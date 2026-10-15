using System.Globalization;

namespace Licoreria.Application.Reportes;

/// <summary>
/// Texto con el que un valor se muestra en un documento (PDF). Usa coma decimal y punto de miles
/// sin depender de la cultura instalada en el servidor.
/// </summary>
public static class FormatoCeldas
{
    private static readonly NumberFormatInfo Numeros = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
        NegativeSign = "-"
    };

    public static string Formatear(object? valor, TipoColumnaReporte tipo)
    {
        if (valor is null)
        {
            return "—";
        }

        if (valor is string texto)
        {
            return texto;
        }

        return (valor, tipo) switch
        {
            (DateTime fecha, TipoColumnaReporte.Fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            (DateTime fecha, _) => fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            (_, TipoColumnaReporte.Entero) => Convert.ToDecimal(valor, CultureInfo.InvariantCulture).ToString("N0", Numeros),
            (_, TipoColumnaReporte.Moneda) => "$ " + Convert.ToDecimal(valor, CultureInfo.InvariantCulture).ToString("N2", Numeros),
            (_, TipoColumnaReporte.Porcentaje) => Convert.ToDecimal(valor, CultureInfo.InvariantCulture).ToString("N2", Numeros) + " %",
            (IFormattable numero, _) => numero.ToString("N2", Numeros),
            _ => valor.ToString() ?? string.Empty
        };
    }

    public static bool EsNumerico(TipoColumnaReporte tipo)
        => tipo is TipoColumnaReporte.Entero or TipoColumnaReporte.Decimal or TipoColumnaReporte.Moneda or TipoColumnaReporte.Porcentaje;
}
