using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de roles del sistema con sus permisos. Requiere el permiso security:roles.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
[Authorize(Policy = Permisos.SeguridadRoles)]
public class RolesController : ControllerBase
{
    private readonly IServicioSeguridad _servicioSeguridad;

    public RolesController(IServicioSeguridad servicioSeguridad)
    {
        _servicioSeguridad = servicioSeguridad;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<RolDto>> ObtenerTodos()
        => Ok(_servicioSeguridad.ObtenerRoles());
}
