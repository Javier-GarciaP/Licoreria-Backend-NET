using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioSeguridad : IServicioSeguridad
{
    public IReadOnlyList<RolDto> ObtenerRoles()
        => Enum.GetValues<RolUsuario>()
               .Select(rol => new RolDto(rol.ToString(), rol.Descripcion(), rol.ObtenerPermisos()))
               .ToList();

    public IReadOnlyList<PermisoDto> ObtenerPermisos()
        => Permisos.Todos
                  .Select(p => new PermisoDto(p.Clave, p.Modulo, p.Descripcion))
                  .ToList();
}
