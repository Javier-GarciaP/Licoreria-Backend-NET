namespace Licoreria.Application.Common;

/// <summary>
/// Opciones de autenticación independientes de la infraestructura.
/// Se enlazan desde la sección "Jwt" de la configuración.
/// </summary>
public sealed class OpcionesAutenticacion
{
    public const string SectionName = "Jwt";

    public int RefreshExpiresDays { get; set; } = 7;
}
