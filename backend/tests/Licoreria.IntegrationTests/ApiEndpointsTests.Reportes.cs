using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>KPIs de ventas y producto: consultas agregadas sobre ventas sembradas y permisos.</summary>
public partial class ApiEndpointsTests
{
    private static string Fecha(DateTime fecha) => Uri.EscapeDataString(fecha.ToString("o"));

    private static string PeriodoDeHoy()
        => $"desde={Fecha(DateTime.UtcNow.AddHours(-1))}&hasta={Fecha(DateTime.UtcNow.AddHours(1))}";

    private static JsonElement? BuscarVariante(JsonElement lista, Guid varianteId)
    {
        foreach (var item in lista.EnumerateArray())
        {
            if (item.GetProperty("varianteId").GetGuid() == varianteId)
            {
                return item;
            }
        }

        return null;
    }

    [HechoDeIntegracion]
    public async Task ReporteProductos_VentasSembradas_AgrupaPorVarianteConRotacionYSinMovimiento()
    {
        await AbrirTurnoAsync();
        var (_, vendida, _) = await CrearProductoConStockAsync("KPI Vendido", 10m, 10);
        var (_, cortesia, _) = await CrearProductoConStockAsync("KPI Cortesía", 4m, 6);
        var (_, quieta, _) = await CrearProductoConStockAsync("KPI Quieto", 3m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new object[]
            {
                new { varianteId = vendida, cantidad = 2 },
                new { varianteId = cortesia, cantidad = 1, esCortesia = true }
            },
            pagos = new[] { new { metodoPagoId, monto = 20.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);

        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/productos?{PeriodoDeHoy()}&limite=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var reporte = await JsonAsync(respuesta);
        var top = reporte.GetProperty("top");

        var filaVendida = BuscarVariante(top, vendida);
        Assert.NotNull(filaVendida);
        Assert.Equal(2m, filaVendida.Value.GetProperty("unidades").GetDecimal());
        Assert.Equal(20m, filaVendida.Value.GetProperty("ingresoUSD").GetDecimal());
        Assert.Equal(8m, filaVendida.Value.GetProperty("existencia").GetDecimal());
        Assert.Equal(0.25m, filaVendida.Value.GetProperty("rotacion").GetDecimal());

        var filaCortesia = BuscarVariante(top, cortesia);
        Assert.NotNull(filaCortesia);
        Assert.Equal(1m, filaCortesia.Value.GetProperty("unidades").GetDecimal());
        Assert.Equal(0m, filaCortesia.Value.GetProperty("ingresoUSD").GetDecimal());

        Assert.Null(BuscarVariante(top, quieta));
        var filaQuieta = BuscarVariante(reporte.GetProperty("sinMovimiento"), quieta);
        Assert.NotNull(filaQuieta);
        Assert.Equal(0m, filaQuieta.Value.GetProperty("rotacion").GetDecimal());
    }

    [HechoDeIntegracion]
    public async Task ReporteProductos_OrdenaPorIngresosYRespetaElLimite()
    {
        var respuesta = await _factory.Client.GetAsync("/api/v1/reportes/productos?orden=Ingresos&limite=1");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var reporte = await JsonAsync(respuesta);
        Assert.True(reporte.GetProperty("top").GetArrayLength() <= 1);
        Assert.Equal(1, reporte.GetProperty("limite").GetInt32());
    }

    [HechoDeIntegracion]
    public async Task ReporteProductos_RangoInvertido_Devuelve400()
    {
        var hoy = DateTime.UtcNow;

        var respuesta = await _factory.Client.GetAsync(
            $"/api/v1/reportes/productos?desde={Fecha(hoy)}&hasta={Fecha(hoy.AddDays(-2))}");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task ReporteProductos_SinPermisoDeVentas_Devuelve403()
    {
        using var host = await ClienteDeAsync("host1@licoreria.com");

        var respuesta = await host.GetAsync("/api/v1/reportes/productos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task ReporteVentas_PagosMixtosUsdYBs_DesglosaPorMetodoYMoneda()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(20m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var tasa = (await JsonAsync(await _factory.Client.GetAsync("/api/v1/tasas-cambio/actual?tipo=Paralelo")))
            .GetProperty("valor").GetDecimal();
        var diezUsdEnBs = Math.Round(10m * tasa, 2);

        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new object[]
            {
                new { metodoPagoId, monto = 10.0, moneda = "USD" },
                new { metodoPagoId, monto = diezUsdEnBs, moneda = "BS" }
            }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);

        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/ventas?{PeriodoDeHoy()}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var reporte = await JsonAsync(respuesta);
        var monedas = reporte.GetProperty("porMetodoPago").EnumerateArray()
            .Select(m => m.GetProperty("moneda").GetString())
            .ToList();
        Assert.Contains("USD", monedas);
        Assert.Contains("BS", monedas);
        var cantidad = reporte.GetProperty("cantidad").GetInt32();
        Assert.True(cantidad >= 1);
        Assert.Equal(
            Math.Round(reporte.GetProperty("totalUSD").GetDecimal() / cantidad, 2),
            reporte.GetProperty("ticketPromedioUSD").GetDecimal());
    }
}
