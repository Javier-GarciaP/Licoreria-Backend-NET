using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Consultas agregadas para reportes y tableros.
/// </summary>
public interface IReportesRepository
{
    Task<DashboardDto> ObtenerDashboardAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        DateTime ahora,
        CancellationToken cancellationToken = default);

    Task<ReportePropinasDto> ObtenerPropinasAsync(
        DateTime desde,
        DateTime hasta,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default);

    Task<ReporteVentasDto> ObtenerVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    Task<InventarioValorizadoDto> ObtenerInventarioValorizadoAsync(
        CancellationToken cancellationToken = default);

    Task<ReporteComprasDto> ObtenerComprasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>Consumo por día de la semana y hora (mapa de calor de horas pico).</summary>
    Task<ReporteHeatmapDto> ObtenerHeatmapAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>Existencias con sus umbrales mín/máx para clasificar la salud del inventario.</summary>
    Task<IReadOnlyList<StockSaludCrudoDto>> ObtenerStockSaludAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Ventas del período y mermas valorizadas para el comparativo.</summary>
    Task<(decimal VentasUSD, IReadOnlyList<ComparativoMermaDto> Mermas)> ObtenerMermasVsVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}
