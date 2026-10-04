using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Eventos del local publicables en la web.</summary>
[ApiController]
[Route("api/v1/eventos")]
public class EventosController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public EventosController(IServicioClub servicio) => _servicio = servicio;

    /// <summary>Eventos publicados (público para la web del cliente).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventoDto>>> ObtenerPublicados(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerEventosAsync(soloPublicados: true, cancellationToken));

    [HttpGet("todos")]
    [Authorize(Policy = Permisos.ContenidoLeer)]
    public async Task<ActionResult<IReadOnlyList<EventoDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerEventosAsync(soloPublicados: false, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<EventoDto>> Crear([FromBody] EventoCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearEventoAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<EventoDto>> Editar(Guid id, [FromBody] EventoEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarEventoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarEventoAsync(id, cancellationToken) ? NoContent() : NotFound();
}
