using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Zonas del local.</summary>
[ApiController]
[Route("api/v1/zonas")]
public class ZonasController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public ZonasController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ZonaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerZonasAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<ZonaDto>> Crear([FromBody] ZonaCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearZonaAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<ZonaDto>> Editar(Guid id, [FromBody] ZonaEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editada = await _servicio.EditarZonaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarZonaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
