using ClosedXML.Excel;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Reportes;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Exporta un reporte a Excel (.xlsx) con ClosedXML: una hoja por tabla, importes y fechas como valores
/// con formato (no texto), encabezado congelado y filtro automático.
/// </summary>
public sealed class ExportadorReportesExcel : IExportadorReportes
{
    private const int LongitudMaximaNombreHoja = 31;
    private const string FormatoMoneda = "\"$\" #,##0.00";

    public FormatoExportacion Formato => FormatoExportacion.Excel;

    public string TipoContenido => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string Extension => "xlsx";

    public byte[] Exportar(DocumentoReporte documento)
    {
        using var libro = new XLWorkbook();
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tabla in documento.Tablas)
        {
            var hoja = libro.Worksheets.Add(NombreDeHoja(tabla.Titulo, nombresUsados));
            DibujarHoja(hoja, documento, tabla);
        }

        if (documento.Tablas.Count == 0)
        {
            DibujarHoja(libro.Worksheets.Add("Reporte"), documento, new TablaReporte("Reporte", [], []));
        }

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static string NombreDeHoja(string titulo, HashSet<string> usados)
    {
        var limpio = new string(titulo.Where(c => !"[]:*?/\\".Contains(c)).ToArray()).Trim();
        if (limpio.Length == 0)
        {
            limpio = "Hoja";
        }

        var nombre = limpio.Length > LongitudMaximaNombreHoja ? limpio[..LongitudMaximaNombreHoja] : limpio;
        var contador = 2;
        while (!usados.Add(nombre))
        {
            var sufijo = $" {contador++}";
            nombre = limpio[..Math.Min(limpio.Length, LongitudMaximaNombreHoja - sufijo.Length)] + sufijo;
        }

        return nombre;
    }

    private static void DibujarHoja(IXLWorksheet hoja, DocumentoReporte documento, TablaReporte tabla)
    {
        var fila = 1;
        hoja.Cell(fila, 1).Value = documento.Local;
        hoja.Cell(fila++, 1).Style.Font.Bold = true;
        hoja.Cell(fila, 1).Value = documento.Titulo;
        hoja.Cell(fila++, 1).Style.Font.SetBold().Font.SetFontSize(14);

        if (documento.Desde is { } desde && documento.Hasta is { } hasta)
        {
            hoja.Cell(fila++, 1).Value = $"Periodo: {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";
        }

        foreach (var (nombre, valor) in documento.Filtros)
        {
            hoja.Cell(fila++, 1).Value = $"{nombre}: {valor}";
        }

        hoja.Cell(fila++, 1).Value = $"Generado el {documento.GeneradoEn:dd/MM/yyyy HH:mm} (UTC) por {documento.GeneradoPor}";
        fila++;

        if (tabla.Columnas.Count == 0)
        {
            return;
        }

        var filaEncabezado = fila;
        for (var c = 0; c < tabla.Columnas.Count; c++)
        {
            var celda = hoja.Cell(filaEncabezado, c + 1);
            celda.Value = tabla.Columnas[c].Titulo;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.LightGray;
            celda.Style.Alignment.Horizontal = FormatoCeldas.EsNumerico(tabla.Columnas[c].Tipo)
                ? XLAlignmentHorizontalValues.Right
                : XLAlignmentHorizontalValues.Left;
        }

        fila++;
        foreach (var datos in tabla.Filas)
        {
            EscribirFila(hoja, fila++, tabla.Columnas, datos, negrita: false);
        }

        var ultimaFilaDatos = fila - 1;
        if (tabla.Totales is { } totales)
        {
            EscribirFila(hoja, fila, tabla.Columnas, totales, negrita: true);
        }

        // El filtro cubre los datos; la fila de totales queda fuera para que no se ordene con ellos.
        var rangoFiltro = hoja.Range(filaEncabezado, 1, Math.Max(ultimaFilaDatos, filaEncabezado), tabla.Columnas.Count);
        rangoFiltro.SetAutoFilter();
        hoja.SheetView.FreezeRows(filaEncabezado);
        hoja.Columns(1, tabla.Columnas.Count).AdjustToContents(filaEncabezado, Math.Min(fila, filaEncabezado + 200));
        hoja.Column(1).Width = Math.Max(hoja.Column(1).Width, 18);
    }

    private static void EscribirFila(
        IXLWorksheet hoja,
        int fila,
        IReadOnlyList<ColumnaReporte> columnas,
        IReadOnlyList<object?> valores,
        bool negrita)
    {
        for (var c = 0; c < columnas.Count; c++)
        {
            var celda = hoja.Cell(fila, c + 1);
            Asignar(celda, valores[c], columnas[c].Tipo);
            if (negrita)
            {
                celda.Style.Font.Bold = true;
            }
        }
    }

    private static void Asignar(IXLCell celda, object? valor, TipoColumnaReporte tipo)
    {
        switch (valor)
        {
            case null:
                return;
            case string texto:
                celda.Value = texto;
                return;
            case DateTime fecha:
                celda.Value = DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
                celda.Style.DateFormat.Format = tipo == TipoColumnaReporte.Fecha ? "dd/mm/yyyy" : "dd/mm/yyyy hh:mm";
                return;
        }

        var numero = Convert.ToDecimal(valor, System.Globalization.CultureInfo.InvariantCulture);
        switch (tipo)
        {
            case TipoColumnaReporte.Porcentaje:
                celda.Value = numero / 100m;
                celda.Style.NumberFormat.Format = "0.00%";
                break;
            case TipoColumnaReporte.Moneda:
                celda.Value = numero;
                celda.Style.NumberFormat.Format = FormatoMoneda;
                break;
            case TipoColumnaReporte.Entero:
                celda.Value = numero;
                celda.Style.NumberFormat.Format = "#,##0";
                break;
            default:
                celda.Value = numero;
                celda.Style.NumberFormat.Format = "#,##0.00";
                break;
        }
    }
}
