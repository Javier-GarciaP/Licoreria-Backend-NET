namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Opciones de almacenamiento local de archivos.
/// </summary>
public sealed class OpcionesAlmacenamiento
{
    public const string SectionName = "Almacenamiento";

    public string RutaBase { get; set; } = "wwwroot/uploads";
    public string UrlBase { get; set; } = "/uploads";
}
