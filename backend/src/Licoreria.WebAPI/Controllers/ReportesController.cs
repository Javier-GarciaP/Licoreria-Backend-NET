using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Reportes;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
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
    private readonly IServicioExportacionReportes _exportacion;
    private readonly IAuthorizationService _autorizacion;

    public ReportesController(
        IServicioInventario inventario,
        IServicioReportes reportes,
        IServicioExportacionReportes exportacion,
        IAuthorizationService autorizacion)
    {
        _inventario = inventario;
        _reportes = reportes;
        _exportacion = exportacion;
        _autorizacion = autorizacion;
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

    /// <summary>Productos más vendidos del periodo con su rotación y los productos sin movimiento.</summary>
    [HttpGet("productos")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ReporteProductosDto>> Productos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] OrdenProductos orden = OrdenProductos.Unidades,
        [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
        => Ok(await _reportes.ObtenerProductosAsync(desde, hasta, orden, limite, cancellationToken));

    /// <summary>Ocupación actual de mesas por zona, permanencia promedio y rotación del periodo.</summary>
    [HttpGet("ocupacion")]
    [Authorize(Policy = Permisos.ClubLeer)]
    public async Task<ActionResult<ReporteOcupacionDto>> Ocupacion(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerOcupacionAsync(desde, hasta, cancellationToken));

    /// <summary>Reservas del periodo por estado y tasa de no asistencia.</summary>
    [HttpGet("reservas-asistencia")]
    [Authorize(Policy = Permisos.ClubLeer)]
    public async Task<ActionResult<ReporteReservasAsistenciaDto>> ReservasAsistencia(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerReservasAsistenciaAsync(desde, hasta, cancellationToken));

    /// <summary>Descuadre de caja por turno cerrado del periodo.</summary>
    [HttpGet("caja")]
    [Authorize(Policy = Permisos.CajaCerrar)]
    public async Task<ActionResult<ReporteCajaDto>> Caja(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerCajaAsync(desde, hasta, cancellationToken));

    /// <summary>Desempeño por mesonero: ventas generadas, cancelaciones y tiempo de servicio.</summary>
    [HttpGet("mesoneros")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReporteMesonerosDto>> Mesoneros(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _reportes.ObtenerMesonerosAsync(desde, hasta, cancellationToken));

    /// <summary>
    /// Descarga un reporte en PDF (<c>formato=pdf</c>) o Excel (<c>formato=xlsx</c>) con los filtros de su consulta.
    /// Exige el mismo permiso que la consulta del reporte.
    /// </summary>
    [HttpGet("{reporte}/exportar")]
    [Authorize]
    [Produces("application/pdf", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> Exportar(
        string reporte,
        [FromQuery] string formato,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] Guid? usuarioId,
        [FromQuery] OrdenProductos orden = OrdenProductos.Unidades,
        [FromQuery] int limite = 10,
        [FromQuery] string? entidad = null,
        [FromQuery] string? accion = null,
        [FromQuery] TipoAuditoria? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var definicion = CatalogoReportes.Buscar(reporte);
        if (definicion is null)
        {
            return NotFound();
        }

        if (!await TienePermisoAsync(definicion))
        {
            return Forbid();
        }

        var archivo = await _exportacion.ExportarAsync(
            definicion.Id,
            formato,
            new FiltrosExportacion(desde, hasta, usuarioId, orden, limite, entidad, accion, tipo),
            cancellationToken);

        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    private async Task<bool> TienePermisoAsync(ReporteExportable reporte)
    {
        if (reporte.SoloAdministrador)
        {
            return User.IsInRole("Admin");
        }

        var resultado = await _autorizacion.AuthorizeAsync(User, reporte.Permiso!);
        return resultado.Succeeded;
    }
}
