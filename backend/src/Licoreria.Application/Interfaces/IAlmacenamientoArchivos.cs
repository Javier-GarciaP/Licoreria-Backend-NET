namespace Licoreria.Application.Interfaces;

/// <summary>
/// Abstracción del almacenamiento de archivos (imágenes, comprobantes, media).
/// </summary>
public interface IAlmacenamientoArchivos
{
    Task<ArchivoAlmacenado> GuardarAsync(
        Stream contenido,
        string nombreOriginal,
        string carpeta,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(string rutaRelativa, CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadatos del archivo almacenado.
/// </summary>
public sealed record ArchivoAlmacenado(
    string RutaRelativa,
    string Url,
    string NombreOriginal,
    long Tamano);
