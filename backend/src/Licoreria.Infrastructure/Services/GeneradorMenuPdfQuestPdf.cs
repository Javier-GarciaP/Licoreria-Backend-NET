using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Genera el menú digital en PDF con QuestPDF (licencia Community).
/// </summary>
public sealed class GeneradorMenuPdfQuestPdf : IGeneradorMenuPdf
{
    static GeneradorMenuPdfQuestPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generar(MenuDigitalDto menu)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(t => t.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Menú").FontSize(24).Bold();
                    col.Item().Text($"Generado: {menu.GeneradoEn:dd/MM/yyyy HH:mm}  ·  Tasa: {menu.Tasa:0.00} Bs/USD")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Content().Column(col =>
                {
                    foreach (var seccion in menu.Secciones)
                    {
                        col.Item().PaddingTop(12).Text(seccion.Nombre)
                            .FontSize(15).Bold().FontColor(Colors.Blue.Darken2);

                        foreach (var item in seccion.Items)
                        {
                            col.Item().PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text(item.Nombre);
                                row.ConstantItem(70).AlignRight().Text($"$ {item.PrecioUSD:0.00}");
                                row.ConstantItem(90).AlignRight().Text($"Bs {item.PrecioBS:0.00}");
                            });
                        }
                    }
                });

                page.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Licorería / Discoteca  ·  Página ");
                    texto.CurrentPageNumber();
                });
            });
        });

        return documento.GeneratePdf();
    }
}
