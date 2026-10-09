using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

[Collection("api")]
public class ApiEndpointsTests
{
    private readonly LicoreriaApiFactory _factory;

    public ApiEndpointsTests(LicoreriaApiFactory factory) => _factory = factory;

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    /// <summary>Abre un turno (caja) si no hay uno abierto, para poder operar cuentas y ventas.</summary>
    private async Task AbrirTurnoAsync()
    {
        var turno = await _factory.Client.GetAsync("/api/v1/sesiones-caja/turno");
        var body = await JsonAsync(turno);
        if (!body.GetProperty("abierto").GetBoolean())
        {
            var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/sesiones-caja", new { fondoInicial = 0 });
            abrir.EnsureSuccessStatusCode();
        }
    }

    /// <summary>Arqueo con una denominación en cero, válido para cerrar la caja.</summary>
    private async Task<object[]> ArqueoCeroAsync()
    {
        var denominaciones = await _factory.Client.GetAsync("/api/v1/denominaciones");
        var body = await JsonAsync(denominaciones);
        return new object[] { new { denominacionId = body[0].GetProperty("id").GetGuid(), cantidad = 0 } };
    }

    /// <summary>Crea una zona y devuelve su id (necesaria para crear mesas).</summary>
    private async Task<Guid> CrearZonaAsync()
    {
        var response = await _factory.Client.PostAsJsonAsync("/api/v1/zonas", new { nombre = "Zona Test", tipo = "Mesas" });
        response.EnsureSuccessStatusCode();
        var body = await JsonAsync(response);
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin@licoreria.com",
            password = "demo123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task EndpointProtegido_SinToken_Devuelve401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Producto_ConVariante_SeCreaYObtiene()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Producto Integración",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Presentación",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 2.0,
                    precioVentaUSD = 5.0,
                    esBase = true,
                    codigosBarras = new[] { $"CB-{Guid.NewGuid().ToString("N")[..10]}" }
                }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var producto = await JsonAsync(response);
        var productoId = producto.GetProperty("id").GetGuid();

        var obtenido = await _factory.Client.GetAsync($"/api/v1/productos/{productoId}");
        Assert.Equal(HttpStatusCode.OK, obtenido.StatusCode);
        var body = await JsonAsync(obtenido);
        Assert.Equal(sku, body.GetProperty("variantes")[0].GetProperty("sku").GetString());
    }

    [Fact]
    public async Task Venta_ConPagoSuficiente_GeneraComprobanteYDescuentaStock()
    {
        await AbrirTurnoAsync();
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 10);

        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 5.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var venta = await JsonAsync(response);
        Assert.Equal(5.0m, venta.GetProperty("totalUSD").GetDecimal());
        Assert.False(string.IsNullOrWhiteSpace(venta.GetProperty("numeroComprobante").GetString()));

        var stock = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={sku}");
        var stockBody = await JsonAsync(stock);
        Assert.Equal(9m, stockBody.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());
    }

    [Fact]
    public async Task Venta_ConPagoInsuficiente_Devuelve422()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 1.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Preparado_ConReceta_AlVender_DescuentaElInsumo()
    {
        await AbrirTurnoAsync();
        var (_, licorVarianteId, licorSku) = await CrearProductoConStockAsync("Licor Para Tragos", 12.0m, 10);

        var (tragoVarianteId, tragoSku) = await CrearPreparadoAsync();
        var receta = await _factory.Client.PostAsJsonAsync($"/api/v1/variantes/{tragoVarianteId}/recetas", new
        {
            varianteInsumoId = licorVarianteId,
            cantidad = 0.25m
        });
        Assert.Equal(HttpStatusCode.Created, receta.StatusCode);

        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId = tragoVarianteId, cantidad = 2 } },
            pagos = new[] { new { metodoPagoId, monto = 16.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);

        // 2 tragos × 0.25 de licor: el stock del insumo baja, no el del trago.
        var stock = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={licorSku}");
        var stockBody = await JsonAsync(stock);
        Assert.Equal(9.5m, stockBody.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());

        // El trago no tiene stock propio: al vender no se le descuenta nada.
        var stockTrago = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={tragoSku}");
        var stockTragoBody = await JsonAsync(stockTrago);
        Assert.Equal(0, stockTragoBody.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Preparado_ConPresentacionBase_Devuelve422()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Trago Con Base",
            categoriaId,
            tipo = "Preparado",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Trago",
                    sku = $"ITP-{Guid.NewGuid().ToString("N")[..10]}",
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 8.0,
                    esBase = true,
                    codigosBarras = Array.Empty<string>()
                }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.Contains("preparado", body.GetProperty("detail").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preparado_ConStockPropio_Devuelve422()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Trago Con Stock",
            categoriaId,
            tipo = "Preparado",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Trago",
                    sku = $"ITS-{Guid.NewGuid().ToString("N")[..10]}",
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 8.0,
                    esBase = false,
                    stockMinimo = 2.0,
                    codigosBarras = Array.Empty<string>()
                }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.Contains("stock", body.GetProperty("detail").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Crea un producto Preparado de Barra sin base ni receta y devuelve su variante y SKU.</summary>
    private async Task<(Guid VarianteId, string Sku)> CrearPreparadoAsync()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"ITP-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Trago Integración",
            categoriaId,
            tipo = "Preparado",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Trago",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 8.0,
                    esBase = false,
                    codigosBarras = Array.Empty<string>()
                }
            }
        });

        response.EnsureSuccessStatusCode();
        var producto = await JsonAsync(response);
        var varianteId = producto.GetProperty("variantes")[0].GetProperty("id").GetGuid();
        return (varianteId, sku);
    }

    [Fact]
    public async Task Dashboard_ComoAdmin_DevuelveIndicadores()
    {
        var response = await _factory.Client.GetAsync("/api/v1/reportes/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.True(body.TryGetProperty("ventasMesUSD", out _));
    }

    [Fact]
    public async Task Modificador_SeCreaYAsignaAProducto()
    {
        var (productoId, _, _) = await CrearProductoConStockAsync("Producto Modificador", 5.0m, 3);

        var creacion = await _factory.Client.PostAsJsonAsync("/api/v1/modificadores", new
        {
            nombre = "Doble hielo",
            precioAdicional = 1.5
        });
        Assert.Equal(HttpStatusCode.Created, creacion.StatusCode);
        var modificador = await JsonAsync(creacion);
        var modificadorId = modificador.GetProperty("id").GetGuid();

        var asignacion = await _factory.Client.PostAsJsonAsync(
            $"/api/v1/productos/{productoId}/modificadores",
            new { modificadorId, minimo = 0, maximo = 2 });
        Assert.Equal(HttpStatusCode.Created, asignacion.StatusCode);
        var asignado = await JsonAsync(asignacion);
        Assert.Equal("Doble hielo", asignado.GetProperty("modificadorNombre").GetString());

        var listado = await _factory.Client.GetAsync($"/api/v1/productos/{productoId}/modificadores");
        var items = await JsonAsync(listado);
        Assert.Equal(1, items.GetArrayLength());

        var quitar = await _factory.Client.DeleteAsync(
            $"/api/v1/productos/{productoId}/modificadores/{asignado.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, quitar.StatusCode);
    }

    [Fact]
    public async Task ReportePropinas_ReflejaLaVentaConPropina()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 5.0, moneda = "USD", propina = 1.5 } }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);

        var response = await _factory.Client.GetAsync("/api/v1/reportes/propinas");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await JsonAsync(response);
        Assert.True(body.GetProperty("totalPropinaUSD").GetDecimal() >= 1.5m);
        Assert.True(body.GetProperty("porUsuario").GetArrayLength() >= 1);
    }

    [Theory]
    [InlineData("/api/v1/reportes/ventas")]
    [InlineData("/api/v1/reportes/inventario")]
    [InlineData("/api/v1/reportes/compras")]
    public async Task Reportes_Operativos_Devuelven200(string ruta)
    {
        var response = await _factory.Client.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Plano_ConElementos_SeCreaYSeRecuperaConSusElementos()
    {
        var creado = await CrearPlanoAsync("Plano Integración");
        var creadoId = creado.GetProperty("id").GetGuid();

        var planos = await ObtenerPlanosAsync();
        var enLista = BuscarEnArray(planos, creadoId);
        Assert.Equal(1, enLista.GetProperty("elementos").GetArrayLength());
        Assert.Equal("mesa_redonda", enLista.GetProperty("elementos")[0].GetProperty("forma").GetString());

        var obtenido = await _factory.Client.GetAsync($"/api/v1/planos/{creadoId}");
        Assert.Equal(HttpStatusCode.OK, obtenido.StatusCode);
        var body = await JsonAsync(obtenido);
        Assert.Equal(1, body.GetProperty("elementos").GetArrayLength());
    }

    [Fact]
    public async Task Plano_AlCrearUnoNuevo_DesactivaElAnterior()
    {
        var primero = await CrearPlanoAsync("Plano Activo Anterior");
        var segundo = await CrearPlanoAsync("Plano Activo Nuevo");
        var primeroId = primero.GetProperty("id").GetGuid();
        var segundoId = segundo.GetProperty("id").GetGuid();

        var planos = await ObtenerPlanosAsync();
        var p1 = BuscarEnArray(planos, primeroId);
        var p2 = BuscarEnArray(planos, segundoId);
        Assert.False(p1.GetProperty("activo").GetBoolean());
        Assert.True(p2.GetProperty("activo").GetBoolean());
    }

    [Fact]
    public async Task Cuenta_AlAbrirMesa_RegistraQuienLaAbrio()
    {
        await AbrirTurnoAsync();
        var yo = await _factory.Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, yo.StatusCode);
        var yoBody = await JsonAsync(yo);
        var miId = yoBody.GetProperty("id").GetGuid();

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new
        {
            nombreMesa = "Mesa 99",
            mesaId = (Guid?)null,
            cliente = "Cliente Prueba",
            notas = "Prefiere la esquina"
        });
        Assert.Equal(HttpStatusCode.Created, abrir.StatusCode);
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var obtenida = await _factory.Client.GetAsync($"/api/v1/cuentas/{cuentaId}");
        Assert.Equal(HttpStatusCode.OK, obtenida.StatusCode);
        var body = await JsonAsync(obtenida);
        Assert.Equal(miId, body.GetProperty("abiertaPorId").GetGuid());
        Assert.Equal("Cliente Prueba", body.GetProperty("cliente").GetString());
    }

    [Fact]
    public async Task Chat_EnviaYObtieneMensaje()
    {
        var enviar = await _factory.Client.PostAsJsonAsync("/api/v1/staff/chat", new { mensaje = "Comanda 5 lista en barra" });
        Assert.Equal(HttpStatusCode.Created, enviar.StatusCode);
        var enviado = await JsonAsync(enviar);
        Assert.Equal("Comanda 5 lista en barra", enviado.GetProperty("mensaje").GetString());

        var listado = await _factory.Client.GetAsync("/api/v1/staff/chat?limit=10");
        Assert.Equal(HttpStatusCode.OK, listado.StatusCode);
        var body = await JsonAsync(listado);
        var encontrado = false;
        for (var i = 0; i < body.GetArrayLength(); i += 1)
        {
            if (body[i].GetProperty("mensaje").GetString() == "Comanda 5 lista en barra")
            {
                encontrado = true;
                break;
            }
        }

        Assert.True(encontrado);
    }

    [Fact]
    public async Task Item_SePuedeMoverAEnProceso()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Kanban" });
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Cocina",
            items = new[] { new { varianteId, cantidad = 1, esCortesia = false } }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);
        var cuerpo = await JsonAsync(comanda);
        var comandaId = cuerpo.GetProperty("comandas")[0].GetProperty("id").GetGuid();
        var detalleId = cuerpo.GetProperty("comandas")[0].GetProperty("detalles")[0].GetProperty("id").GetGuid();

        var mover = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado",
            new { estado = "EnProceso" });
        Assert.Equal(HttpStatusCode.OK, mover.StatusCode);
        var movido = await JsonAsync(mover);
        Assert.Equal("EnProceso", movido.GetProperty("comandas")[0].GetProperty("detalles")[0].GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Cuenta_SinTurnoAbierto_NoSePuedeAbrirMesa()
    {
        var turno = await _factory.Client.GetAsync("/api/v1/sesiones-caja/turno");
        var cuerpo = await JsonAsync(turno);
        if (cuerpo.GetProperty("abierto").GetBoolean())
        {
            var activa = await _factory.Client.GetAsync("/api/v1/sesiones-caja/activa");
            var sesion = await JsonAsync(activa);
            var arqueo = await ArqueoCeroAsync();
            await _factory.Client.PostAsJsonAsync(
                $"/api/v1/sesiones-caja/{sesion.GetProperty("id").GetGuid()}/cerrar",
                new { arqueo });
        }

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Sin Turno" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, abrir.StatusCode);
    }

    [Fact]
    public async Task Cuenta_Reabrir_DevuelveAOcupada()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);

        var zonaId = await CrearZonaAsync();
        var crearMesa = await _factory.Client.PostAsJsonAsync("/api/v1/mesas", new
        {
            zonaId,
            numero = $"R-{Guid.NewGuid().ToString("N")[..4]}",
            capacidad = 4
        });
        Assert.Equal(HttpStatusCode.Created, crearMesa.StatusCode);
        var mesa = await JsonAsync(crearMesa);
        var mesaId = mesa.GetProperty("id").GetGuid();

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Reabrir", mesaId });
        Assert.Equal(HttpStatusCode.Created, abrir.StatusCode);
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = new[] { new { varianteId, cantidad = 1, esCortesia = false } }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);
        var conConsumo = await JsonAsync(comanda);
        Assert.Equal("Abierta", conConsumo.GetProperty("estado").GetString());

        var desalojar = await _factory.Client.PostAsJsonAsync($"/api/v1/mesas/{mesaId}/desalojar", new { });
        Assert.Equal(HttpStatusCode.NoContent, desalojar.StatusCode);

        var desalojada = await _factory.Client.GetAsync($"/api/v1/cuentas/{cuentaId}");
        var cuerpo = await JsonAsync(desalojada);
        Assert.Equal("PorCobrar", cuerpo.GetProperty("estado").GetString());

        var reabrir = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/reabrir", new { });
        Assert.Equal(HttpStatusCode.OK, reabrir.StatusCode);
        var reabierta = await JsonAsync(reabrir);
        Assert.Equal("Abierta", reabierta.GetProperty("estado").GetString());

        var mesas = await _factory.Client.GetAsync("/api/v1/mesas");
        var mesasBody = await JsonAsync(mesas);
        var mesaOperativa = false;
        for (var i = 0; i < mesasBody.GetArrayLength(); i += 1)
        {
            if (mesasBody[i].GetProperty("id").GetGuid() == mesaId)
            {
                Assert.Equal(cuentaId, mesasBody[i].GetProperty("cuentaId").GetGuid());
                mesaOperativa = true;
                break;
            }
        }
        Assert.True(mesaOperativa);
    }

    [Fact]
    public async Task Turno_AlCerrar_DesalojaLasMesasYDejaPorCobrar()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Cierre Turno" });
        Assert.Equal(HttpStatusCode.Created, abrir.StatusCode);
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Cocina",
            items = new[] { new { varianteId, cantidad = 1, esCortesia = false } }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);

        var activa = await _factory.Client.GetAsync("/api/v1/sesiones-caja/activa");
        var sesion = await JsonAsync(activa);
        var sesionId = sesion.GetProperty("id").GetGuid();

        var cerrar = await _factory.Client.PostAsJsonAsync($"/api/v1/sesiones-caja/{sesionId}/cerrar", new { arqueo = await ArqueoCeroAsync() });
        Assert.Equal(HttpStatusCode.OK, cerrar.StatusCode);
        var cerrada = await JsonAsync(cerrar);
        Assert.Equal("Cerrada", cerrada.GetProperty("estado").GetString());

        var obtenida = await _factory.Client.GetAsync($"/api/v1/cuentas/{cuentaId}");
        Assert.Equal(HttpStatusCode.OK, obtenida.StatusCode);
        var cuerpo = await JsonAsync(obtenida);
        Assert.Equal("PorCobrar", cuerpo.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Comanda_ConStockInsuficiente_Devuelve422()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 2);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Sin Stock" });
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = new[] { new { varianteId, cantidad = 5 } }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, comanda.StatusCode);
    }

    [Fact]
    public async Task Item_AlServirse_DescuentaStockYAlCerrarNoDuplica()
    {
        await AbrirTurnoAsync();
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 10);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Servida" });
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = new[] { new { varianteId, cantidad = 1 } }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);
        var cuerpo = await JsonAsync(comanda);
        var comandaId = cuerpo.GetProperty("comandas")[0].GetProperty("id").GetGuid();
        var detalleId = cuerpo.GetProperty("comandas")[0].GetProperty("detalles")[0].GetProperty("id").GetGuid();

        var mover = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado",
            new { estado = "Preparado" });
        Assert.Equal(HttpStatusCode.OK, mover.StatusCode);

        var trasServir = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={sku}");
        var cuerpoStock = await JsonAsync(trasServir);
        Assert.Equal(9m, cuerpoStock.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());

        var cerrar = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/cerrar", new
        {
            pagos = new[] { new { metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago"), monto = 5.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.OK, cerrar.StatusCode);

        var trasCerrar = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={sku}");
        var cuerpoFinal = await JsonAsync(trasCerrar);
        Assert.Equal(9m, cuerpoFinal.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());
    }

    [Fact]
    public async Task Item_AlCancelarse_ReduceElTotalDeLaCuenta()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 10);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Cancelar" });
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = new[]
            {
                new { varianteId, cantidad = 1 },
                new { varianteId, cantidad = 1 }
            }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);
        var cuerpo = await JsonAsync(comanda);
        Assert.Equal(10m, cuerpo.GetProperty("saldo").GetDecimal());
        var comandaId = cuerpo.GetProperty("comandas")[0].GetProperty("id").GetGuid();
        var detalleId = cuerpo.GetProperty("comandas")[0].GetProperty("detalles")[0].GetProperty("id").GetGuid();

        var cancelar = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado",
            new { estado = "Cancelado" });
        Assert.Equal(HttpStatusCode.OK, cancelar.StatusCode);
        var cancelado = await JsonAsync(cancelar);
        Assert.Equal(5m, cancelado.GetProperty("saldo").GetDecimal());
    }

    [Fact]
    public async Task Producto_ConVarianteNoBaseYStockInicial_CreaExistencia()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var skuBase = $"ITB-{Guid.NewGuid().ToString("N")[..10]}";
        var skuVaso = $"ITV-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Barra con stock en presentación",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Barra",
            variantes = new object[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Botella",
                    sku = skuBase,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 2.0,
                    precioVentaUSD = 5.0,
                    esBase = true
                },
                new
                {
                    id = (Guid?)null,
                    nombre = "Vaso",
                    sku = skuVaso,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 0.5,
                    precioVentaUSD = 2.0,
                    esBase = false,
                    stockInicial = 7
                }
            }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var stock = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={skuVaso}");
        var cuerpo = await JsonAsync(stock);
        Assert.Equal(7m, cuerpo.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());
    }

    [Fact]
    public async Task Producto_CocinaSinStock_SePuedePedirYServirSinKardex()
    {
        await AbrirTurnoAsync();
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var crear = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Comida Sin Stock",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Cocina",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Plato",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 4.0,
                    esBase = false
                }
            }
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var producto = await JsonAsync(crear);
        var varianteId = producto.GetProperty("variantes")[0].GetProperty("id").GetGuid();

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Cocina" });
        var cuenta = await JsonAsync(abrir);
        var cuentaId = cuenta.GetProperty("id").GetGuid();

        var comanda = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Cocina",
            items = new[] { new { varianteId, cantidad = 2 } }
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);

        var cuerpo = await JsonAsync(comanda);
        var comandaId = cuerpo.GetProperty("comandas")[0].GetProperty("id").GetGuid();
        var detalleId = cuerpo.GetProperty("comandas")[0].GetProperty("detalles")[0].GetProperty("id").GetGuid();

        var mover = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado",
            new { estado = "Preparado" });
        Assert.Equal(HttpStatusCode.OK, mover.StatusCode);

        var stock = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={sku}");
        var cuerpoStock = await JsonAsync(stock);
        Assert.Equal(0, cuerpoStock.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Producto_BarraSimpleSinBase_Devuelve422()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Licor Sin Base",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Vaso",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 3.0,
                    esBase = false
                }
            }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Producto_CocinaConStockInicial_Devuelve422()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Cocina Con Stock",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Cocina",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Plato",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = 3.0,
                    esBase = false,
                    stockInicial = 5
                }
            }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private async Task<JsonElement> ObtenerPlanosAsync()
    {
        var response = await _factory.Client.GetAsync("/api/v1/planos");
        response.EnsureSuccessStatusCode();
        return await JsonAsync(response);
    }

    private static JsonElement BuscarEnArray(JsonElement array, Guid id)
    {
        for (var i = 0; i < array.GetArrayLength(); i += 1)
        {
            if (array[i].GetProperty("id").GetGuid() == id)
            {
                return array[i];
            }
        }

        throw new Exception("Plano no encontrado en el listado");
    }

    private async Task<JsonElement> CrearPlanoAsync(string nombre)
    {
        var response = await _factory.Client.PostAsJsonAsync("/api/v1/planos", new
        {
            nombre,
            elementos = new[]
            {
                new
                {
                    zonaId = (Guid?)null,
                    mesaId = (Guid?)null,
                    tipo = "mesa",
                    forma = "mesa_redonda",
                    color = (string?)null,
                    etiqueta = (string?)null,
                    z = 0,
                    posX = 4.0,
                    posY = 4.0,
                    ancho = 2.0,
                    alto = 2.0,
                    rotacion = 0
                }
            }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await JsonAsync(response);
        return json;
    }

    private async Task<Guid> ObtenerPrimerIdAsync(string url)
    {
        var response = await _factory.Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var body = await JsonAsync(response);
        return body[0].GetProperty("id").GetGuid();
    }

    private async Task<(Guid VarianteId, string Sku)> CrearProductoConStockAsync(decimal precioVenta, int stock)
    {
        var (_, varianteId, sku) = await CrearProductoConStockAsync("Producto Venta", precioVenta, stock);
        return (varianteId, sku);
    }

    private async Task<(Guid ProductoId, Guid VarianteId, string Sku)> CrearProductoConStockAsync(
        string nombre,
        decimal precioVenta,
        int stock)
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var productoResponse = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre,
            categoriaId,
            tipo = "Simple",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Presentación",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = precioVenta,
                    esBase = true,
                    codigosBarras = new[] { $"CB-{Guid.NewGuid().ToString("N")[..10]}" }
                }
            }
        });

        productoResponse.EnsureSuccessStatusCode();
        var producto = await JsonAsync(productoResponse);
        var productoId = producto.GetProperty("id").GetGuid();
        var varianteId = producto.GetProperty("variantes")[0].GetProperty("id").GetGuid();

        var ajuste = await _factory.Client.PostAsJsonAsync("/api/v1/ajustes-inventario", new
        {
            varianteId,
            cantidad = stock,
            motivo = "Carga de prueba"
        });
        ajuste.EnsureSuccessStatusCode();

        return (productoId, varianteId, sku);
    }
}
