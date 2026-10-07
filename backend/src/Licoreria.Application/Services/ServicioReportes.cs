using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioReportes : IServicioReportes
{
    private static readonly TimeSpan RangoPorDefecto = TimeSpan.FromDays(30);

    private readonly IReportesRepository _reportes;
    private readonly IRelojSistema _reloj;
    private readonly EvaluadorSaludStock _evaluadorSaludStock;

    public ServicioReportes(
        IReportesRepository reportes,
        IRelojSistema reloj,
        EvaluadorSaludStock evaluadorSaludStock)
    {
        _reportes = reportes;
        _reloj = reloj;
        _evaluadorSaludStock = evaluadorSaludStock;
    }

    public Task<DashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var ahora = _reloj.UtcNow;
        var inicioDia = ahora.Date;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return _reportes.ObtenerDashboardAsync(inicioDia, inicioMes, ahora, cancellationToken);
    }

    public Task<ReportePropinasDto> ObtenerPropinasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        return _reportes.ObtenerPropinasAsync(inicio, fin, usuarioId, cancellationToken);
    }

    public Task<ReporteVentasDto> ObtenerVentasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        return _reportes.ObtenerVentasAsync(inicio, fin, cancellationToken);
    }

    public Task<InventarioValorizadoDto> ObtenerInventarioValorizadoAsync(CancellationToken cancellationToken = default)
        => _reportes.ObtenerInventarioValorizadoAsync(cancellationToken);

    public Task<ReporteComprasDto> ObtenerComprasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        return _reportes.ObtenerComprasAsync(inicio, fin, cancellationToken);
    }

    public Task<ReporteHeatmapDto> ObtenerHeatmapAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        return _reportes.ObtenerHeatmapAsync(inicio, fin, cancellationToken);
    }

    public async Task<DiagnosticoInventarioDto> ObtenerDiagnosticoInventarioAsync(CancellationToken cancellationToken = default)
    {
        var stock = await _reportes.ObtenerStockSaludAsync(cancellationToken);

        var items = stock
            .Select(s =>
            {
                var informe = _evaluadorSaludStock.Evaluar(
                    s.Sku,
                    (int)s.Cantidad,
                    (int)s.StockMinimo,
                    (int)s.StockMaximo);

                return new DiagnosticoInventarioItemDto(
                    s.VarianteId,
                    s.Sku,
                    s.ProductoNombre,
                    s.VarianteNombre,
                    s.Cantidad,
                    s.StockMinimo,
                    s.StockMaximo,
                    informe.Estado.ToString(),
                    informe.MensajeDiagnostico,
                    informe.UnidadesCompraSugeridas);
            })
            .OrderBy(i => i.Estado)
            .ThenBy(i => i.ProductoNombre)
            .ToList();

        int Contar(EstadoSaludStock estado) => items.Count(i => i.Estado == estado.ToString());

        return new DiagnosticoInventarioDto(
            items.Count,
            Contar(EstadoSaludStock.SinStock),
            Contar(EstadoSaludStock.RiesgoCritico),
            Contar(EstadoSaludStock.Subabastecido),
            Contar(EstadoSaludStock.Optimo),
            Contar(EstadoSaludStock.Sobreabastecido),
            Contar(EstadoSaludStock.Excesivo),
            items);
    }

    public async Task<ReporteMermasVsVentasDto> ObtenerMermasVsVentasAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        var (ventasUsd, mermas) = await _reportes.ObtenerMermasVsVentasAsync(inicio, fin, cancellationToken);
        var mermaValorizada = mermas.Sum(m => m.ValorUSD);
        var porcentaje = ventasUsd <= 0m ? 0m : Math.Round(mermaValorizada / ventasUsd * 100m, 2);

        return new ReporteMermasVsVentasDto(
            inicio,
            fin,
            ventasUsd,
            mermaValorizada,
            porcentaje,
            mermas);
    }

    private (DateTime Inicio, DateTime Fin) NormalizarRango(DateTime? desde, DateTime? hasta)
    {
        var fin = hasta ?? _reloj.UtcNow;
        var inicio = desde ?? fin.Subtract(RangoPorDefecto);
        return (inicio, fin);
    }
}
