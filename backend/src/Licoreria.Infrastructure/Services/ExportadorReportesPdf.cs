using Licoreria.Application.Interfaces;
using Licoreria.Application.Reportes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Exporta un reporte a PDF con QuestPDF (licencia Community): A4, horizontal con más de 6 columnas,
/// encabezado de página y de tabla repetidos y pie con numeración.
/// </summary>
public sealed class ExportadorReportesPdf : IExportadorReportes
{
    private const int ColumnasMaximasVertical = 6;

    static ExportadorReportesPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public FormatoExportacion Formato => FormatoExportacion.Pdf;

    public string TipoContenido => "application/pdf";

    public string Extension => "pdf";

    public byte[] Exportar(DocumentoReporte documento) => Construir(documento).GeneratePdf();

    public IDocument Construir(DocumentoReporte reporte)
    {
        var horizontal = reporte.Tablas.Any(t => t.Columnas.Count > ColumnasMaximasVertical);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(horizontal ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().PaddingBottom(8).Column(col =>
                {
                    col.Item().Text(reporte.Local).FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text(reporte.Titulo).FontSize(18).Bold();
                    col.Item().Text(DescribirPeriodoYFiltros(reporte)).FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(14);
                    foreach (var tabla in reporte.Tablas)
                    {
                        col.Item().Element(c => DibujarTabla(c, tabla));
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Generado el {reporte.GeneradoEn:dd/MM/yyyy HH:mm} (UTC) por {reporte.GeneradoPor}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.AutoItem().Text(texto =>
                    {
                        texto.DefaultTextStyle(t => t.FontSize(7).FontColor(Colors.Grey.Darken1));
                        texto.Span("Página ");
                        texto.CurrentPageNumber();
                        texto.Span(" de ");
                        texto.TotalPages();
                    });
                });
            });
        });
    }

    private static string DescribirPeriodoYFiltros(DocumentoReporte reporte)
    {
        var partes = new List<string>();

        if (reporte.Desde is { } desde && reporte.Hasta is { } hasta)
        {
            partes.Add($"Periodo: {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}");
        }

        partes.AddRange(reporte.Filtros.Select(f => $"{f.Nombre}: {f.Valor}"));
        return partes.Count == 0 ? "Sin filtros" : string.Join("  ·  ", partes);
    }

    private static void DibujarTabla(IContainer container, TablaReporte tabla)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(4).Text(tabla.Titulo).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

            if (tabla.Filas.Count == 0)
            {
                col.Item().Text("Sin datos en el periodo.").FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(def =>
                {
                    for (var i = 0; i < tabla.Columnas.Count; i++)
                    {
                        def.RelativeColumn(i == 0 && tabla.Columnas[i].Tipo == TipoColumnaReporte.Texto ? 2f : 1f);
                    }
                });

                t.Header(header =>
                {
                    foreach (var columna in tabla.Columnas)
                    {
                        Alinear(header.Cell().Background(Colors.Grey.Lighten3).Padding(3), columna.Tipo)
                            .Text(columna.Titulo).Bold();
                    }
                });

                foreach (var fila in tabla.Filas)
                {
                    for (var i = 0; i < tabla.Columnas.Count; i++)
                    {
                        var celda = t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3);
                        Alinear(celda, tabla.Columnas[i].Tipo)
                            .Text(FormatoCeldas.Formatear(fila[i], tabla.Columnas[i].Tipo));
                    }
                }

                if (tabla.Totales is { } totales)
                {
                    for (var i = 0; i < tabla.Columnas.Count; i++)
                    {
                        var celda = t.Cell().BorderTop(1).BorderColor(Colors.Grey.Darken1).Padding(3);
                        var valor = totales[i] is null ? string.Empty : FormatoCeldas.Formatear(totales[i], tabla.Columnas[i].Tipo);
                        Alinear(celda, tabla.Columnas[i].Tipo).Text(valor).Bold();
                    }
                }
            });
        });
    }

    private static IContainer Alinear(IContainer container, TipoColumnaReporte tipo)
        => FormatoCeldas.EsNumerico(tipo) ? container.AlignRight() : container;
}
