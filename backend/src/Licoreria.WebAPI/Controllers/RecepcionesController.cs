using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Recepciones de mercancía que actualizan inventario y costos.</summary>
[ApiController]
[Route("api/v1/recepciones")]
[Authorize(Policy = Permisos.ComprasLeer)]
public class RecepcionesController : ControllerBase
{
    private readonly IServicioCompras _servicio;

    public RecepcionesController(IServicioCompras servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<RecepcionDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? ordenCompraId,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerRecepcionesAsync(paginacion, ordenCompraId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecepcionDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var recepcion = await _servicio.ObtenerRecepcionAsync(id, cancellationToken);
        return recepcion is null ? NotFound() : Ok(recepcion);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<RecepcionDto>> Registrar(
        [FromBody] RegistrarRecepcionDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarRecepcionAsync(dto, cancellationToken));
}
