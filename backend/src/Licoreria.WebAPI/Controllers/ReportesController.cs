using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Reportes de operación y tablero de indicadores.</summary>
[ApiController]
[Route("api/v1/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IServicioInventario _inventario;
    private readonly IServicioReportes _reportes;

    public ReportesController(IServicioInventario inventario, IServicioReportes reportes)
    {
        _inventario = inventario;
        _reportes = reportes;
    }

    [HttpGet("mermas")]
    [Authorize(Policy = Permisos.InventarioLeer)]
    public async Task<ActionResult<ReporteMermaDto>> Mermas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var inicio = desde ?? DateTime.UtcNow.AddDays(-30);
        var fin = hasta ?? DateTime.UtcNow;
        return Ok(await _inventario.ObtenerReporteMermasAsync(inicio, fin, cancellationToken));
    }

    /// <summary>Tablero de KPIs del administrador.</summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerDashboardAsync(cancellationToken));
}
