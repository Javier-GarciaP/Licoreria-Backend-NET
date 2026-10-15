using System.Net;
using System.Net.Http.Json;
using ClosedXML.Excel;

namespace Licoreria.IntegrationTests;

/// <summary>Descarga de reportes en PDF y Excel: contenido, nombre, permisos y errores.</summary>
public partial class ApiEndpointsTests
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static string Periodo(string desde = "2026-10-01", string hasta = "2026-10-13")
        => $"desde={desde}&hasta={hasta}";

    [HechoDeIntegracion]
    public async Task Exportar_VentasEnExcel_DescargaUnLibroConImportesNumericosYNombreDelPeriodo()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(8m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");
        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 8.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);
        var hoy = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/ventas/exportar?formato=xlsx&{Periodo(hoy, hoy)}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(TipoExcel, respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Equal($"reporte-ventas-{hoy}_{hoy}.xlsx", respuesta.Content.Headers.ContentDisposition?.FileNameStar
            ?? respuesta.Content.Headers.ContentDisposition?.FileName);

        using var libro = new XLWorkbook(await respuesta.Content.ReadAsStreamAsync());
        var resumen = libro.Worksheet("Resumen");
        var encabezado = resumen.CellsUsed(c => c.GetString() == "Total USD").Single();
        var celdaTotal = resumen.Cell(encabezado.Address.RowNumber + 1, encabezado.Address.ColumnNumber);
        Assert.Equal(XLDataType.Number, celdaTotal.DataType);
        Assert.True(celdaTotal.GetDouble() >= 8d);
    }

    [HechoDeIntegracion]
    public async Task Exportar_InventarioEnPdf_DescargaUnPdf()
    {
        var respuesta = await _factory.Client.GetAsync("/api/v1/reportes/inventario/exportar?formato=pdf");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/pdf", respuesta.Content.Headers.ContentType?.MediaType);
        var bytes = await respuesta.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
        var nombre = respuesta.Content.Headers.ContentDisposition?.FileNameStar ?? respuesta.Content.Headers.ContentDisposition?.FileName;
        Assert.StartsWith("reporte-inventario-", nombre);
        Assert.EndsWith(".pdf", nombre);
    }

    [TeoriaDeIntegracion]
    [InlineData("productos")]
    [InlineData("propinas")]
    [InlineData("compras")]
    [InlineData("caja")]
    [InlineData("ocupacion")]
    [InlineData("mermas-vs-ventas")]
    [InlineData("mesoneros")]
    [InlineData("auditoria")]
    public async Task Exportar_CadaReporte_GeneraPdfYExcelParaElAdministrador(string reporte)
    {
        var pdf = await _factory.Client.GetAsync($"/api/v1/reportes/{reporte}/exportar?formato=pdf");
        var excel = await _factory.Client.GetAsync($"/api/v1/reportes/{reporte}/exportar?formato=xlsx");

        Assert.True(pdf.IsSuccessStatusCode, await pdf.Content.ReadAsStringAsync());
        Assert.True(excel.IsSuccessStatusCode, await excel.Content.ReadAsStringAsync());
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType?.MediaType);
        Assert.Equal(TipoExcel, excel.Content.Headers.ContentType?.MediaType);
    }

    [HechoDeIntegracion]
    public async Task Exportar_ComprasSinPermisoDeCompras_Devuelve403()
    {
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        var respuesta = await cajero.GetAsync("/api/v1/reportes/compras/exportar?formato=xlsx");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [TeoriaDeIntegracion]
    [InlineData("mesoneros")]
    [InlineData("auditoria")]
    public async Task Exportar_ReportesDeAdministrador_NoSeDescarganComoCajero(string reporte)
    {
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        var respuesta = await cajero.GetAsync($"/api/v1/reportes/{reporte}/exportar?formato=pdf");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Exportar_FormatoDesconocido_Devuelve400()
    {
        var respuesta = await _factory.Client.GetAsync("/api/v1/reportes/ventas/exportar?formato=csv");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Exportar_ReporteInexistente_Devuelve404()
    {
        var respuesta = await _factory.Client.GetAsync("/api/v1/reportes/inexistente/exportar?formato=pdf");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Exportar_SinSesion_Devuelve401()
    {
        using var anonimo = _factory.CreateClient();

        var respuesta = await anonimo.GetAsync("/api/v1/reportes/ventas/exportar?formato=pdf");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Exportar_PeriodoInvertido_Devuelve400()
    {
        var respuesta = await _factory.Client.GetAsync(
            $"/api/v1/reportes/ventas/exportar?formato=pdf&{Periodo("2026-10-13", "2026-10-01")}");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
