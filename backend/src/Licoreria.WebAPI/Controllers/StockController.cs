using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Existencias por variante con alertas de stock mínimo.</summary>
[ApiController]
[Route("api/v1/stock")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class StockController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public StockController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<StockDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] bool soloBajoMinimo,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerStockAsync(paginacion, soloBajoMinimo, busqueda, cancellationToken));
}
