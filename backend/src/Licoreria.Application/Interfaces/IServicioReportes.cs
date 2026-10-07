using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de reportes y tableros.
/// </summary>
public interface IServicioReportes
{
    Task<DashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default);

    Task<ReportePropinasDto> ObtenerPropinasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task<ReporteVentasDto> ObtenerVentasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<InventarioValorizadoDto> ObtenerInventarioValorizadoAsync(CancellationToken cancellationToken = default);

    Task<ReporteComprasDto> ObtenerComprasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<ReporteHeatmapDto> ObtenerHeatmapAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<DiagnosticoInventarioDto> ObtenerDiagnosticoInventarioAsync(
        CancellationToken cancellationToken = default);

    Task<ReporteMermasVsVentasDto> ObtenerMermasVsVentasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);
}
