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

    /// <summary>
    /// Unidades e ingresos por variante en ventas no anuladas del periodo, junto con la existencia actual.
    /// Incluye las variantes con existencia aunque no tengan ventas.
    /// </summary>
    Task<IReadOnlyList<ProductoVendidoCrudoDto>> ObtenerProductosVendidosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mesas activas y ocupadas ahora, y sesiones iniciadas en el periodo, agregadas por zona.
    /// Las sesiones abiertas cuentan su permanencia hasta <paramref name="ahora"/>.
    /// </summary>
    Task<IReadOnlyList<OcupacionZonaCrudaDto>> ObtenerOcupacionAsync(
        DateTime desde,
        DateTime hasta,
        DateTime ahora,
        CancellationToken cancellationToken = default);

    /// <summary>Reservas del periodo agrupadas por estado.</summary>
    Task<IReadOnlyList<ReservasPorEstadoDto>> ObtenerReservasPorEstadoAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>Turnos de caja cerrados en el periodo, con su arqueo.</summary>
    Task<IReadOnlyList<TurnoCajaDto>> ObtenerTurnosCerradosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actividad de comandas del periodo agrupada por el usuario que las creó
    /// (los ítems de comandas sin creador se agrupan como «Sin asignar»).
    /// </summary>
    Task<IReadOnlyList<MesoneroCrudoDto>> ObtenerActividadMesonerosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}
