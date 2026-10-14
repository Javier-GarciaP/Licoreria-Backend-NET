using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de permisos del sistema. Requiere el permiso security:roles.
/// </summary>
[ApiController]
[Route("api/v1/permisos")]
[Authorize(Policy = Permisos.SeguridadRoles)]
public class PermisosController : ControllerBase
{
    private readonly IServicioSeguridad _servicioSeguridad;

    public PermisosController(IServicioSeguridad servicioSeguridad)
    {
        _servicioSeguridad = servicioSeguridad;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<PermisoDto>> ObtenerTodos()
        => Ok(_servicioSeguridad.ObtenerPermisos());
}
