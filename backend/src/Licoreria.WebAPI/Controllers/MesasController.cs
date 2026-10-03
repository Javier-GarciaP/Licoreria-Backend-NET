using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Mesas del local y su disponibilidad.</summary>
[ApiController]
[Route("api/v1/mesas")]
public class MesasController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public MesasController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MesaDto>>> Obtener([FromQuery] Guid? zonaId, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMesasAsync(zonaId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<MesaDto>> Crear([FromBody] MesaCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearMesaAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<MesaDto>> Editar(Guid id, [FromBody] MesaEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editada = await _servicio.EditarMesaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarMesaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
