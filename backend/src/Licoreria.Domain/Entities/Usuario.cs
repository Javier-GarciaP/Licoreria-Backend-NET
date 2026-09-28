using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Usuario : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cajero"; // Administrador, Cajero
    public bool Activo { get; set; } = true;
}
