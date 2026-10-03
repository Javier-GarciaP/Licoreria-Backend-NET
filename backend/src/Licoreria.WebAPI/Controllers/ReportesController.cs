using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Reportes de operación.</summary>
[ApiController]
[Route("api/v1/reportes")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class ReportesController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public ReportesController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet("mermas")]
    public async Task<ActionResult<ReporteMermaDto>> Mermas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var inicio = desde ?? DateTime.UtcNow.AddDays(-30);
        var fin = hasta ?? DateTime.UtcNow;
        return Ok(await _servicio.ObtenerReporteMermasAsync(inicio, fin, cancellationToken));
    }
}
