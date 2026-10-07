using Licoreria.Domain.Enums;

namespace Licoreria.Application.Security;

/// <summary>
/// Mapea los roles del dominio a los roles de seguridad usados en el JWT y en [Authorize],
/// y define la matriz rol → permisos.
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

    /// <summary>Descripción legible de cada rol de dominio.</summary>
    public static string Descripcion(this RolUsuario rol) => rol switch
    {
        RolUsuario.Administrador => "Control total del sistema.",
        RolUsuario.Cajero => "POS, abonos, cierre de cuentas y caja.",
        RolUsuario.Mesero => "Mesas, comandas, cuentas y reservas asignadas.",
        RolUsuario.Barra => "Comandas de barra y su estado.",
        RolUsuario.Cocina => "Comandas de cocina y su estado.",
        RolUsuario.Host => "Reservas y asignación de mesas.",
        RolUsuario.EditorContenido => "Contenido de la web pública y eventos.",
        _ => "Rol del sistema."
    };

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

    /// <summary>
    /// Devuelve los permisos concedidos a un rol, según la matriz RBAC del proyecto.
    /// El administrador recibe todos los permisos.
    /// </summary>
    public static IReadOnlyList<string> ObtenerPermisos(this RolUsuario rol)
    {
        if (rol == RolUsuario.Administrador)
        {
            return Permisos.Todos.Select(p => p.Clave).ToList();
        }

        return rol switch
        {
            RolUsuario.Cajero =>
            [
                Permisos.CatalogoLeer,
                Permisos.InventarioLeer,
                Permisos.InventarioMerma,
                Permisos.VentasLeer,
                Permisos.VentasEscribir,
                Permisos.CuentasLeer,
                Permisos.CuentasAbonar,
                Permisos.CuentasCerrar,
                Permisos.CajaAbrir,
                Permisos.CajaCerrar,
                Permisos.CajaMovimiento,
                Permisos.ReservasGestionar,
                Permisos.FinanzasLeer
            ],
            RolUsuario.Mesero =>
            [
                Permisos.CatalogoLeer,
                Permisos.InventarioLeer,
                Permisos.InventarioMerma,
                Permisos.VentasLeer,
                Permisos.VentasEscribir,
                Permisos.CuentasLeer,
                Permisos.ClubLeer,
                Permisos.ReservasGestionar
            ],
            RolUsuario.Barra =>
            [
                Permisos.CatalogoLeer,
                Permisos.InventarioLeer
            ],
            RolUsuario.Cocina =>
            [
                Permisos.CatalogoLeer,
                Permisos.InventarioLeer
            ],
            RolUsuario.Host =>
            [
                Permisos.CatalogoLeer,
                Permisos.ClubLeer,
                Permisos.ClubGestionar,
                Permisos.ReservasGestionar
            ],
            RolUsuario.EditorContenido =>
            [
                Permisos.CatalogoLeer,
                Permisos.ContenidoLeer,
                Permisos.ContenidoPublicar
            ],
            _ => [Permisos.CatalogoLeer]
        };
    }
}
