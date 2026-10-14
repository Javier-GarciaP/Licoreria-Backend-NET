using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Kardex de movimientos de inventario (inmutable, filtrable).</summary>
[ApiController]
[Route("api/v1/movimientos-inventario")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class MovimientosInventarioController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public MovimientosInventarioController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<MovimientoKardexDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? varianteId,
        [FromQuery] TipoMovimientoInventario? tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMovimientosAsync(paginacion, varianteId, tipo, desde, hasta, cancellationToken));
}
