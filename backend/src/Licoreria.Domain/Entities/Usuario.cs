using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

public class Usuario : BaseEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Cajero;
    public bool Activo { get; set; } = true;

    public void ActualizarDatos(string nombreCompleto, string email, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new InvalidOperationException("El nombre completo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("El correo es obligatorio.");
        }

        NombreCompleto = nombreCompleto;
        Email = email;
        Rol = rol;
        MarcarModificado();
    }

    public void CambiarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidOperationException("El hash de la contraseña es obligatorio.");
        }

        PasswordHash = passwordHash;
        MarcarModificado();
    }

    public void Activar()
    {
        Activo = true;
        MarcarModificado();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificado();
    }
}
