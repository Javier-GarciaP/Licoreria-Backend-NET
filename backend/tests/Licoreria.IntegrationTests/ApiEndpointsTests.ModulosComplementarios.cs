using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>Permisos de promociones y de CRM por rol, y venta con cliente y fiado.</summary>
public partial class ApiEndpointsTests
{
    private static object PromocionNueva(string nombre) => new { nombre, tipo = "Porcentaje", valor = 10m, activo = true };

    [Fact]
    public async Task Promociones_MeseroNoPuedeCrear_CajeroListaYAdministradorCrea()
    {
        using var mesero = await ClienteDeAsync("mesero1@licoreria.com");
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        var creaMesero = await mesero.PostAsJsonAsync("/api/v1/promociones", PromocionNueva("Mesero"));
        var creaCajero = await cajero.PostAsJsonAsync("/api/v1/promociones", PromocionNueva("Cajero"));
        var listaCajero = await cajero.GetAsync("/api/v1/promociones");
        var creaAdmin = await _factory.Client.PostAsJsonAsync("/api/v1/promociones", PromocionNueva("Admin"));

        Assert.Equal(HttpStatusCode.Forbidden, creaMesero.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, creaCajero.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listaCajero.StatusCode);
        Assert.True(creaAdmin.IsSuccessStatusCode, await creaAdmin.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Clientes_CajeroPuedeListar_MeseroRecibe403()
    {
        using var mesero = await ClienteDeAsync("mesero1@licoreria.com");
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        Assert.Equal(HttpStatusCode.OK, (await cajero.GetAsync("/api/v1/clientes")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await mesero.GetAsync("/api/v1/clientes")).StatusCode);
    }

    // ================= Venta con cliente, puntos y fiado =================

    private async Task<Guid> MetodoCreditoIdAsync()
    {
        var metodos = await JsonAsync(await _factory.Client.GetAsync("/api/v1/metodos-pago"));
        return metodos.EnumerateArray().First(m => m.GetProperty("codigo").GetString() == "Credito").GetProperty("id").GetGuid();
    }

    private async Task<Guid> CrearClienteVentaAsync(string nombre)
    {
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/clientes", new { nombre });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    private async Task<JsonElement> ClienteAsync(Guid id)
        => await JsonAsync(await _factory.Client.GetAsync($"/api/v1/clientes/{id}"));

    [Fact]
    public async Task Venta_ConCliente_AcumulaPuntosYQuedaAsociada()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(12.5m, 10);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var clienteId = await CrearClienteVentaAsync("Cliente Puntos");

        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            clienteId,
            items = new[] { new { varianteId, cantidad = 2 } },
            pagos = new[] { new { metodoPagoId, monto = 25.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var venta = await JsonAsync(respuesta);
        Assert.Equal(clienteId, venta.GetProperty("clienteId").GetGuid());
        Assert.Equal(25, venta.GetProperty("puntosGanados").GetInt32());
        Assert.Equal(25, (await ClienteAsync(clienteId)).GetProperty("puntos").GetInt32());
    }

    [Fact]
    public async Task Venta_ConFiado_CreaCuentaPorCobrarEnlazadaYSumaPuntosSoloDeLoPagado()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(40m, 10);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var credito = await MetodoCreditoIdAsync();
        var clienteId = await CrearClienteVentaAsync("Cliente Fiado");

        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            clienteId,
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[]
            {
                new { metodoPagoId, monto = 30.0, moneda = "USD" },
                new { metodoPagoId = credito, monto = 10.0, moneda = "USD" }
            }
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var venta = await JsonAsync(respuesta);
        Assert.Equal(10m, venta.GetProperty("creditoUSD").GetDecimal());
        Assert.Equal(30, venta.GetProperty("puntosGanados").GetInt32());

        var cuentas = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas-por-cobrar?clienteId={clienteId}"));
        var cuenta = Assert.Single(cuentas.GetProperty("items").EnumerateArray());
        Assert.Equal(10m, cuenta.GetProperty("saldoUSD").GetDecimal());
        Assert.Equal(venta.GetProperty("id").GetGuid(), cuenta.GetProperty("ventaId").GetGuid());
    }

    [Fact]
    public async Task Venta_ConCreditoSinCliente_Devuelve422YNoDescuentaStock()
    {
        await AbrirTurnoAsync();
        var (varianteId, sku) = await CrearProductoConStockAsync(10m, 5);
        var credito = await MetodoCreditoIdAsync();

        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId = credito, monto = 10.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        Assert.Equal(5m, (await ObtenerStockAsync(sku)).Cantidad);
    }

    [Fact]
    public async Task Venta_ConClienteInactivoOInexistente_RechazaLaVenta()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(10m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var clienteId = await CrearClienteVentaAsync("Cliente Inactivo");
        var editar = await _factory.Client.PutAsJsonAsync($"/api/v1/clientes/{clienteId}", new { id = clienteId, nombre = "Cliente Inactivo", activo = false });
        editar.EnsureSuccessStatusCode();

        object Cuerpo(Guid cliente) => new
        {
            clienteId = cliente,
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 10.0, moneda = "USD" } }
        };

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await _factory.Client.PostAsJsonAsync("/api/v1/ventas", Cuerpo(clienteId))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _factory.Client.PostAsJsonAsync("/api/v1/ventas", Cuerpo(Guid.NewGuid()))).StatusCode);
    }

    [Fact]
    public async Task Devolucion_RevierteLosPuntosYAvisaSiLaVentaEstaFiada()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(20m, 10);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var credito = await MetodoCreditoIdAsync();
        var clienteId = await CrearClienteVentaAsync("Cliente Devolución");

        var venta = await JsonAsync(await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            clienteId,
            items = new[] { new { varianteId, cantidad = 2 } },
            pagos = new[]
            {
                new { metodoPagoId, monto = 30.0, moneda = "USD" },
                new { metodoPagoId = credito, monto = 10.0, moneda = "USD" }
            }
        }));
        var ventaId = venta.GetProperty("id").GetGuid();
        Assert.Equal(30, (await ClienteAsync(clienteId)).GetProperty("puntos").GetInt32());

        var devolucion = await _factory.Client.PostAsJsonAsync($"/api/v1/ventas/{ventaId}/devoluciones", new
        {
            motivo = "Defecto",
            reintegrarInventario = false,
            detalles = new[] { new { varianteId, cantidad = 1 } }
        });

        Assert.True(devolucion.IsSuccessStatusCode, await devolucion.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace((await JsonAsync(devolucion)).GetProperty("aviso").GetString()));
        Assert.Equal(10, (await ClienteAsync(clienteId)).GetProperty("puntos").GetInt32());
    }

    [Fact]
    public async Task Cliente_Ventas_ListaLasVentasAsociadasYRespetaElPermiso()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(8m, 10);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var clienteId = await CrearClienteVentaAsync("Cliente Historial");
        var venta = await JsonAsync(await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            clienteId,
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 8.0, moneda = "USD" } }
        }));

        var historial = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/clientes/{clienteId}/ventas"));
        var item = Assert.Single(historial.GetProperty("items").EnumerateArray());
        Assert.Equal(venta.GetProperty("id").GetGuid(), item.GetProperty("id").GetGuid());
        Assert.Equal(8m, item.GetProperty("totalUSD").GetDecimal());

        using var mesero = await ClienteDeAsync("mesero1@licoreria.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await mesero.GetAsync($"/api/v1/clientes/{clienteId}/ventas")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _factory.Client.GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/ventas")).StatusCode);
    }

    // ================= Compras: recorrido completo =================

    [Fact]
    public async Task Compras_OrdenAprobadaYRecibida_AumentaElStockYGeneraLaCuentaPorPagar()
    {
        var (_, varianteId, sku) = await CrearProductoConStockAsync("Producto Compras", 9m, 2);

        var proveedor = await _factory.Client.PostAsJsonAsync("/api/v1/proveedores", new { nombre = $"Proveedor {Guid.NewGuid():N}"[..20], diasCredito = 15 });
        Assert.Equal(HttpStatusCode.Created, proveedor.StatusCode);
        var proveedorId = (await JsonAsync(proveedor)).GetProperty("id").GetGuid();

        var orden = await _factory.Client.PostAsJsonAsync("/api/v1/ordenes-compra", new
        {
            proveedorId,
            detalles = new[] { new { varianteId, cantidad = 10, costoUnitarioUSD = 4.0 } }
        });
        Assert.Equal(HttpStatusCode.Created, orden.StatusCode);
        var ordenJson = await JsonAsync(orden);
        var ordenId = ordenJson.GetProperty("id").GetGuid();
        var detalleId = ordenJson.GetProperty("detalles")[0].GetProperty("id").GetGuid();

        Assert.True((await _factory.Client.PostAsync($"/api/v1/ordenes-compra/{ordenId}/aprobar", null)).IsSuccessStatusCode);

        var recepcion = await _factory.Client.PostAsJsonAsync("/api/v1/recepciones", new
        {
            ordenCompraId = ordenId,
            detalles = new[] { new { ordenCompraDetalleId = detalleId, cantidad = 10 } }
        });
        Assert.True(recepcion.IsSuccessStatusCode, await recepcion.Content.ReadAsStringAsync());

        Assert.Equal(12m, (await ObtenerStockAsync(sku)).Cantidad);

        var cuentas = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas-por-pagar?proveedorId={proveedorId}"));
        var cuenta = Assert.Single(cuentas.GetProperty("items").EnumerateArray());
        Assert.Equal(40m, cuenta.GetProperty("montoUSD").GetDecimal());

        var pago = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas-por-pagar/{cuenta.GetProperty("id").GetGuid()}/pagos", new { monto = 40.0, moneda = "USD" });
        Assert.True(pago.IsSuccessStatusCode, await pago.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Compras_SoloElAdministradorLasConsulta()
    {
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await cajero.GetAsync("/api/v1/ordenes-compra")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cajero.GetAsync("/api/v1/proveedores")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _factory.Client.GetAsync("/api/v1/ordenes-compra")).StatusCode);
    }
}
