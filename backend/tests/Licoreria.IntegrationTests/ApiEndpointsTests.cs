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

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin@licoreria.com",
            password = "admin123"
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
