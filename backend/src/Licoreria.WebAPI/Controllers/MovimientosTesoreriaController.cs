using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Movimientos de tesorería (ingresos y egresos de fondos).</summary>
[ApiController]
[Route("api/v1/movimientos-tesoreria")]
[Authorize(Policy = Permisos.FinanzasLeer)]
public class MovimientosTesoreriaController : ControllerBase
{
    private readonly IServicioFinanzas _servicio;

    public MovimientosTesoreriaController(IServicioFinanzas servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<MovimientoTesoreriaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] TipoMovimientoTesoreria? tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMovimientosAsync(paginacion, tipo, desde, hasta, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.FinanzasTasa)]
    public async Task<ActionResult<MovimientoTesoreriaDto>> Registrar(
        [FromBody] RegistrarMovimientoTesoreriaDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarMovimientoAsync(dto, cancellationToken));
}
