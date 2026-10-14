using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Entradas de acceso (cover) con control por código/QR.</summary>
[ApiController]
[Route("api/v1/entradas")]
[Authorize(Policy = Permisos.ClubGestionar)]
public class EntradasController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public EntradasController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<EntradaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? eventoId,
        [FromQuery] EstadoEntrada? estado,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerEntradasAsync(paginacion, eventoId, estado, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EntradaDto>> Emitir([FromBody] EmitirEntradaDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.EmitirEntradaAsync(dto, cancellationToken));

    /// <summary>Valida (marca como usada) una entrada por su código.</summary>
    [HttpPost("{codigo}/validar")]
    public async Task<ActionResult<EntradaDto>> Validar(string codigo, CancellationToken cancellationToken)
    {
        var entrada = await _servicio.ValidarEntradaAsync(codigo, cancellationToken);
        return entrada is null ? NotFound() : Ok(entrada);
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
        => await _servicio.CancelarEntradaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
