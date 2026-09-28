using Licoreria.Domain.Enums;

namespace Licoreria.Application.Security;

/// <summary>
/// Mapea los roles del dominio a los roles de seguridad usados en el JWT y en [Authorize].
/// Se conserva el rol de dominio y se agrega el rol de seguridad equivalente
/// (Admin / Employee) exigido por las directivas del proyecto.
/// </summary>
public static class RolesSeguridad
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";

    public const string Administrador = "Administrador";
    public const string Cajero = "Cajero";
    public const string Mesero = "Mesero";
    public const string Barra = "Barra";
    public const string Cocina = "Cocina";
    public const string Host = "Host";
    public const string EditorContenido = "EditorContenido";

    public static IReadOnlyList<string> ObtenerRoles(this RolUsuario rol) => rol switch
    {
        RolUsuario.Administrador => [Admin, Administrador],
        RolUsuario.Cajero => [Employee, Cajero],
        RolUsuario.Mesero => [Employee, Mesero],
        RolUsuario.Barra => [Employee, Barra],
        RolUsuario.Cocina => [Employee, Cocina],
        RolUsuario.Host => [Employee, Host],
        RolUsuario.EditorContenido => [Employee, EditorContenido],
        _ => [Employee]
    };
}
