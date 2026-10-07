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

    /// <summary>Propinas registradas por usuario que facturó.</summary>
    [HttpGet("propinas")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ReportePropinasDto>> Propinas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] Guid? usuarioId,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerPropinasAsync(desde, hasta, usuarioId, cancellationToken));

    /// <summary>Ventas del período con desglose por día, usuario y método de pago.</summary>
    [HttpGet("ventas")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ReporteVentasDto>> Ventas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerVentasAsync(desde, hasta, cancellationToken));

    /// <summary>Inventario valorizado a costo y a precio de venta.</summary>
    [HttpGet("inventario")]
    [Authorize(Policy = Permisos.InventarioLeer)]
    public async Task<ActionResult<InventarioValorizadoDto>> Inventario(CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerInventarioValorizadoAsync(cancellationToken));

    /// <summary>Compras del período y situación de cuentas por pagar.</summary>
    [HttpGet("compras")]
    [Authorize(Policy = Permisos.ComprasLeer)]
    public async Task<ActionResult<ReporteComprasDto>> Compras(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerComprasAsync(desde, hasta, cancellationToken));

    /// <summary>Mapa de calor de consumo por día de la semana y hora (horas pico).</summary>
    [HttpGet("heatmap")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ReporteHeatmapDto>> Heatmap(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerHeatmapAsync(desde, hasta, cancellationToken));

    /// <summary>Diagnóstico de salud del inventario (mínimos y máximos) con sugerencias de compra.</summary>
    [HttpGet("inventario-salud")]
    [Authorize(Policy = Permisos.InventarioLeer)]
    public async Task<ActionResult<DiagnosticoInventarioDto>> InventarioSalud(CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerDiagnosticoInventarioAsync(cancellationToken));

    /// <summary>Comparativo de mermas valorizadas frente a las ventas del período.</summary>
    [HttpGet("mermas-vs-ventas")]
    [Authorize(Policy = Permisos.InventarioLeer)]
    public async Task<ActionResult<ReporteMermasVsVentasDto>> MermasVsVentas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerMermasVsVentasAsync(desde, hasta, cancellationToken));
}
