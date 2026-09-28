using System.Text.RegularExpressions;

namespace Licoreria.Domain.Services;

/// <summary>
/// Resultado de la generación y sanitización de un SKU corporativo.
/// </summary>
public sealed record ResultadoGeneracionSku(
    string EntradaOriginal,
    string Categoria,
    string NombreSanitizado,
    string SkuGenerado,
    string FormulaSku,
    bool EsValidoEmpresarial);

/// <summary>
/// Genera SKUs corporativos con el formato [CAT]-[PRO]-[SECUENCIA_4D].
/// Sanitiza la entrada, extrae prefijos de 3 caracteres y valida la estructura.
/// </summary>
public sealed class GeneradorSku
{
    private const string Formula = "[CAT]-[PRO]-[SECUENCIA_4D]";

    private static readonly Regex CaracteresInvalidos =
        new(@"[^A-Z0-9]", RegexOptions.Compiled);

    private static readonly Regex PatronSku =
        new(@"^[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{4}$", RegexOptions.Compiled);

    public ResultadoGeneracionSku Generar(string categoria, string producto, int secuencia)
    {
        var categoriaSanitizada = Sanitizar(categoria);
        var productoSanitizado = Sanitizar(producto);

        var prefijoCategoria = ExtraerPrefijo(categoriaSanitizada, 'X');
        var prefijoProducto = ExtraerPrefijo(productoSanitizado, 'P');
        var numeroSecuencia = Math.Clamp(secuencia, 1, 9999).ToString("D4");

        var sku = $"{prefijoCategoria}-{prefijoProducto}-{numeroSecuencia}";

        return new ResultadoGeneracionSku(
            $"{categoria} {producto}".Trim(),
            categoria,
            productoSanitizado,
            sku,
            Formula,
            PatronSku.IsMatch(sku));
    }

    /// <summary>
    /// Elimina caracteres especiales, signos de puntuación y espacios.
    /// </summary>
    private static string Sanitizar(string valor)
        => CaracteresInvalidos.Replace((valor ?? string.Empty).ToUpperInvariant(), string.Empty);

    /// <summary>
    /// Extrae los primeros 3 caracteres, rellenando si la longitud es menor.
    /// </summary>
    private static string ExtraerPrefijo(string sanitizado, char relleno)
        => sanitizado.PadRight(3, relleno)[..3];
}
