using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Catálogo de roles y permisos del sistema (solo lectura).
/// </summary>
public interface IServicioSeguridad
{
    IReadOnlyList<RolDto> ObtenerRoles();

    IReadOnlyList<PermisoDto> ObtenerPermisos();
}
