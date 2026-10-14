namespace Licoreria.Application.Common;

/// <summary>
/// Valida la imagen de fondo de un plano: extensión, tipo declarado, tamaño y firma de archivo.
/// </summary>
public static class ValidadorImagenFondo
{
    public const long TamanoMaximo = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> TiposPorExtension = new()
    {
        [".png"] = ["image/png"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".webp"] = ["image/webp"]
    };

    /// <summary>Devuelve el motivo del rechazo o <c>null</c> si la imagen es válida.</summary>
    public static string? Validar(string nombreArchivo, string? tipoContenido, long tamano, ReadOnlySpan<byte> cabecera)
    {
        if (tamano <= 0)
        {
            return "Debe adjuntar un archivo con contenido.";
        }

        if (tamano > TamanoMaximo)
        {
            return "La imagen no puede superar los 5 MB.";
        }

        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        if (!TiposPorExtension.TryGetValue(extension, out var tipos))
        {
            return "Solo se permiten imágenes PNG, JPG o WebP.";
        }

        if (string.IsNullOrWhiteSpace(tipoContenido) || !tipos.Contains(tipoContenido.ToLowerInvariant()))
        {
            return "El tipo del archivo no coincide con su extensión.";
        }

        return CoincideFirma(extension, cabecera)
            ? null
            : "El contenido del archivo no corresponde a una imagen del formato indicado.";
    }

    private static bool CoincideFirma(string extension, ReadOnlySpan<byte> c) => extension switch
    {
        ".png" => c.Length >= 8 && c[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        ".jpg" or ".jpeg" => c.Length >= 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF,
        ".webp" => c.Length >= 12
            && c[..4].SequenceEqual("RIFF"u8)
            && c.Slice(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };
}
