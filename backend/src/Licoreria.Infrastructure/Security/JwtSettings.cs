namespace Licoreria.Infrastructure.Security;

/// <summary>
/// Configuración del token JWT leída desde la sección "Jwt" de appsettings
/// o desde variables de entorno (Jwt__Key, Jwt__Issuer, ...).
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
}
