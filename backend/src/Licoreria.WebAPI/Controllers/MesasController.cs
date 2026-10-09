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
    public async Task<ActionResult<IReadOnlyList<MesaDto>>> Obtener(
        [FromQuery] Guid? zonaId,
        [FromQuery] DateTime? fechaHora,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMesasAsync(zonaId, fechaHora, cancellationToken));

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
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editada = await _servicio.EditarMesaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarMesaAsync(id, cancellationToken) ? NoContent() : NotFound();

    /// <summary>
    /// Marca la mesa como desalojada. Cierra la sesión, deja la cuenta pendiente de cobro
    /// si tiene saldo y notifica por SignalR que la mesa quedó libre para ser ocupada.
    /// </summary>
    [HttpPost("{id:guid}/desalojar")]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<IActionResult> Desalojar(Guid id, CancellationToken cancellationToken)
        => await _servicio.DesalojarMesaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
