using ClosedXML.Excel;
using Licoreria.Application.Reportes;
using Licoreria.Infrastructure.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Licoreria.UnitTests;

/// <summary>Exportadores PDF y Excel a partir del modelo neutral de reporte.</summary>
public sealed class ExportadoresReportesTests
{
    private static readonly DateTime Desde = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Hasta = new(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);

    private static TablaReporte TablaDeVentas(int filas = 3)
        => new(
            "Por día",
            [
                new("Fecha", TipoColumnaReporte.Fecha),
                new("Ventas", TipoColumnaReporte.Entero),
                new("Total USD", TipoColumnaReporte.Moneda),
                new("Margen", TipoColumnaReporte.Porcentaje),
                new("Peso", TipoColumnaReporte.Decimal),
                new("Cierre", TipoColumnaReporte.FechaHora),
                new("Nota", TipoColumnaReporte.Texto)
            ],
            Enumerable.Range(0, filas)
                .Select(i => (IReadOnlyList<object?>)new object?[]
                {
                    Desde.AddDays(i), i + 1, 10.5m * (i + 1), 12.5m, 1.25m, Desde.AddDays(i).AddHours(23), i % 2 == 0 ? "ok" : null
                })
                .ToList(),
            ["Total", filas, 10.5m * filas, null, null, null, null]);

    private static DocumentoReporte Documento(params TablaReporte[] tablas)
        => new("Licorería Test", "Ventas", Desde, Hasta, [("Orden", "Unidades")],
            new DateTime(2026, 10, 14, 15, 30, 0, DateTimeKind.Utc), "admin@licoreria.com", tablas);

    // ---------- PDF (5.2) ----------

    [Fact]
    public void Pdf_ResultadoEmpiezaConLaFirmaPdf()
    {
        var bytes = new ExportadorReportesPdf().Exportar(Documento(TablaDeVentas()));

        Assert.True(bytes.Length > 500);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void Pdf_MasDeSeisColumnas_UsaPaginaHorizontal()
    {
        var pdf = new ExportadorReportesPdf();

        var ancho7 = Dimensiones(pdf.Construir(Documento(TablaDeVentas())));
        var tablaCorta = new TablaReporte("Corta", [new("A", TipoColumnaReporte.Texto), new("B", TipoColumnaReporte.Entero)], [["x", 1]]);
        var ancho2 = Dimensiones(pdf.Construir(Documento(tablaCorta)));

        Assert.True(ancho7.Ancho > ancho7.Alto, "Con 7 columnas la página debe ser horizontal.");
        Assert.True(ancho2.Alto > ancho2.Ancho, "Con 2 columnas la página debe ser vertical.");
    }

    [Fact]
    public void Pdf_ReporteLargo_OcupaVariasPaginas()
    {
        var documento = new ExportadorReportesPdf().Construir(Documento(TablaDeVentas(filas: 160)));

        var paginas = documento.GenerateImages().Count();

        Assert.True(paginas > 1, $"Se esperaban varias páginas y hubo {paginas}.");
    }

    [Fact]
    public void Pdf_TablaSinFilas_NoFalla()
    {
        var vacia = new TablaReporte("Vacía", [new("A", TipoColumnaReporte.Texto)], []);

        var bytes = new ExportadorReportesPdf().Exportar(Documento(vacia));

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    private static (int Ancho, int Alto) Dimensiones(IDocument documento)
    {
        var png = documento.GenerateImages().First();
        // Cabecera PNG: firma (8) + longitud/tipo IHDR (8) y luego ancho y alto de 4 bytes en big endian.
        int Leer(int pos) => (png[pos] << 24) | (png[pos + 1] << 16) | (png[pos + 2] << 8) | png[pos + 3];
        return (Leer(16), Leer(20));
    }

    // ---------- Excel (5.3) ----------

    private static XLWorkbook Abrir(byte[] bytes) => new(new MemoryStream(bytes));

    [Fact]
    public void Excel_ImportesCantidadesYFechasSonValoresNoTexto()
    {
        var bytes = new ExportadorReportesExcel().Exportar(Documento(TablaDeVentas()));

        using var libro = Abrir(bytes);
        var hoja = libro.Worksheet("Por día");
        var encabezado = hoja.CellsUsed(c => c.GetString() == "Total USD").Single().Address.RowNumber;
        var primera = encabezado + 1;

        Assert.Equal(XLDataType.DateTime, hoja.Cell(primera, 1).DataType);
        Assert.Equal(XLDataType.Number, hoja.Cell(primera, 2).DataType);
        Assert.Equal(XLDataType.Number, hoja.Cell(primera, 3).DataType);
        Assert.Equal(10.5, hoja.Cell(primera, 3).GetDouble());
        Assert.Equal(0.125, hoja.Cell(primera, 4).GetDouble(), 6);
        Assert.Contains("%", hoja.Cell(primera, 4).Style.NumberFormat.Format);
        Assert.Equal(XLDataType.DateTime, hoja.Cell(primera, 6).DataType);
    }

    [Fact]
    public void Excel_EncabezadoCongeladoYFiltroAutomatico()
    {
        var bytes = new ExportadorReportesExcel().Exportar(Documento(TablaDeVentas(filas: 5)));

        using var libro = Abrir(bytes);
        var hoja = libro.Worksheet("Por día");
        var encabezado = hoja.CellsUsed(c => c.GetString() == "Ventas" && c.Address.ColumnNumber == 2).Single().Address.RowNumber;

        Assert.True(hoja.AutoFilter.IsEnabled);
        Assert.Equal(encabezado, hoja.AutoFilter.Range.FirstRow().RowNumber());
        Assert.Equal(encabezado + 5, hoja.AutoFilter.Range.LastRow().RowNumber());
        Assert.Equal(encabezado, hoja.SheetView.SplitRow);
    }

    [Fact]
    public void Excel_IncluyeMetadatosTotalesYUnaHojaPorTabla()
    {
        var resumen = new TablaReporte("Resumen", [new("Total USD", TipoColumnaReporte.Moneda)], [[31.5m]]);

        var bytes = new ExportadorReportesExcel().Exportar(Documento(resumen, TablaDeVentas()));

        using var libro = Abrir(bytes);
        Assert.Equal(["Resumen", "Por día"], libro.Worksheets.Select(h => h.Name));
        var hoja = libro.Worksheet("Por día");
        var texto = string.Join('|', hoja.CellsUsed().Select(c => c.GetString()));
        Assert.Contains("Licorería Test", texto);
        Assert.Contains("Periodo: 01/10/2026 al 13/10/2026", texto);
        Assert.Contains("Orden: Unidades", texto);
        Assert.Contains("admin@licoreria.com", texto);
        var filaTotal = hoja.CellsUsed(c => c.GetString() == "Total").Single().Address.RowNumber;
        Assert.Equal(31.5, hoja.Cell(filaTotal, 3).GetDouble());
    }

    [Fact]
    public void Excel_NombresDeHojaLargosORepetidosSeAjustan()
    {
        var larga = new TablaReporte(new string('A', 40), [new("X", TipoColumnaReporte.Texto)], [["a"]]);

        var bytes = new ExportadorReportesExcel().Exportar(Documento(larga, larga));

        using var libro = Abrir(bytes);
        var nombres = libro.Worksheets.Select(h => h.Name).ToList();
        Assert.Equal(2, nombres.Distinct().Count());
        Assert.All(nombres, n => Assert.True(n.Length <= 31));
    }
}
