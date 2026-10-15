using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioReportes : IServicioReportes
{
    private static readonly TimeSpan RangoPorDefecto = TimeSpan.FromDays(30);

    /// <summary>Días máximos que puede abarcar un periodo de reporte.</summary>
    public const int DiasMaximosPeriodo = 366;

    /// <summary>Cantidad máxima de productos que devuelve el ranking.</summary>
    public const int LimiteMaximoTop = 100;

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
            mermas
                .Select(m => m with { PorcentajeVentas = Porcentaje(m.ValorUSD, ventasUsd) })
                .ToList());
    }

    public async Task<ReporteCajaDto> ObtenerCajaAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        var turnos = await _reportes.ObtenerTurnosCerradosAsync(inicio, fin, cancellationToken);

        var total = turnos.Sum(t => t.Descuadre);
        var promedio = turnos.Count == 0 ? 0m : Math.Round(total / turnos.Count, 2);
        var mayor = turnos.Count == 0
            ? 0m
            : turnos.OrderByDescending(t => Math.Abs(t.Descuadre)).First().Descuadre;

        return new ReporteCajaDto(inicio, fin, turnos.Count, total, promedio, mayor, turnos);
    }

    public async Task<ReporteProductosDto> ObtenerProductosAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        OrdenProductos orden = OrdenProductos.Unidades,
        int limite = 10,
        CancellationToken cancellationToken = default)
    {
        if (limite is < 1 or > LimiteMaximoTop)
        {
            throw new ArgumentException($"El límite debe estar entre 1 y {LimiteMaximoTop}.");
        }

        if (!Enum.IsDefined(orden))
        {
            throw new ArgumentException("El orden debe ser Unidades o Ingresos.");
        }

        var (inicio, fin) = NormalizarRango(desde, hasta);
        var filas = await _reportes.ObtenerProductosVendidosAsync(inicio, fin, cancellationToken);

        var vendidos = filas.Where(f => f.Unidades > 0m || f.IngresoUSD > 0m).ToList();
        var totalUnidades = vendidos.Sum(f => f.Unidades);
        var totalIngreso = vendidos.Sum(f => f.IngresoUSD);

        ProductoVendidoDto Mapear(ProductoVendidoCrudoDto f)
        {
            var vendido = f.Unidades > 0m || f.IngresoUSD > 0m;
            var (estado, rotacion) = (vendido, f.Existencia > 0m) switch
            {
                (true, true) => (EstadoRotacion.Calculada, (decimal?)Math.Round(f.Unidades / f.Existencia, 2)),
                (true, false) => (EstadoRotacion.NoCalculable, null),
                _ => (EstadoRotacion.SinMovimiento, 0m)
            };

            return new ProductoVendidoDto(
                f.VarianteId,
                f.Sku,
                f.ProductoNombre,
                f.VarianteNombre,
                f.Unidades,
                f.IngresoUSD,
                Porcentaje(f.Unidades, totalUnidades),
                Porcentaje(f.IngresoUSD, totalIngreso),
                f.Existencia,
                rotacion,
                estado);
        }

        var ordenados = orden == OrdenProductos.Ingresos
            ? vendidos.OrderByDescending(f => f.IngresoUSD).ThenByDescending(f => f.Unidades)
            : vendidos.OrderByDescending(f => f.Unidades).ThenByDescending(f => f.IngresoUSD);

        var top = ordenados.ThenBy(f => f.ProductoNombre).Take(limite).Select(Mapear).ToList();

        var sinMovimiento = filas
            .Where(f => f.Unidades <= 0m && f.IngresoUSD <= 0m && f.Existencia > 0m)
            .OrderByDescending(f => f.Existencia)
            .Select(Mapear)
            .ToList();

        return new ReporteProductosDto(inicio, fin, orden, limite, totalUnidades, totalIngreso, top, sinMovimiento);
    }

    public async Task<ReporteOcupacionDto> ObtenerOcupacionAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        var ahora = _reloj.UtcNow;
        var zonas = await _reportes.ObtenerOcupacionAsync(inicio, fin, ahora, cancellationToken);

        static decimal? Promedio(decimal total, int cantidad)
            => cantidad == 0 ? null : Math.Round(total / cantidad, 2);

        var porZona = zonas
            .Select(z => new OcupacionZonaDto(
                z.ZonaId,
                z.Zona,
                z.MesasActivas,
                z.MesasOcupadas,
                Porcentaje(z.MesasOcupadas, z.MesasActivas),
                z.Sesiones,
                z.SesionesAbiertas,
                Promedio(z.Sesiones, z.MesasActivas),
                Promedio(z.MinutosPermanencia, z.Sesiones)))
            .ToList();

        var mesas = zonas.Sum(z => z.MesasActivas);
        var ocupadas = zonas.Sum(z => z.MesasOcupadas);
        var sesiones = zonas.Sum(z => z.Sesiones);

        return new ReporteOcupacionDto(
            inicio,
            fin,
            ahora,
            mesas,
            ocupadas,
            Porcentaje(ocupadas, mesas),
            sesiones,
            zonas.Sum(z => z.SesionesAbiertas),
            Promedio(sesiones, mesas),
            Promedio(zonas.Sum(z => z.MinutosPermanencia), sesiones),
            porZona);
    }

    public async Task<ReporteReservasAsistenciaDto> ObtenerReservasAsistenciaAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        var porEstado = await _reportes.ObtenerReservasPorEstadoAsync(inicio, fin, cancellationToken);

        int Cantidad(EstadoReserva estado) => porEstado.Where(e => e.Estado == estado).Sum(e => e.Cantidad);

        var asistieron = Cantidad(EstadoReserva.Asistio);
        var noAsistieron = Cantidad(EstadoReserva.NoAsistio);
        var debieronConcretarse = asistieron + noAsistieron;

        return new ReporteReservasAsistenciaDto(
            inicio,
            fin,
            porEstado.Sum(e => e.Cantidad),
            asistieron,
            noAsistieron,
            Cantidad(EstadoReserva.Cancelada),
            debieronConcretarse == 0 ? null : Porcentaje(noAsistieron, debieronConcretarse),
            porEstado);
    }

    public async Task<ReporteMesonerosDto> ObtenerMesonerosAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fin) = NormalizarRango(desde, hasta);
        var actividad = await _reportes.ObtenerActividadMesonerosAsync(inicio, fin, cancellationToken);

        var mesoneros = actividad
            .Select(a => new MesoneroDto(
                a.UsuarioId,
                a.Nombre,
                a.Comandas,
                a.ItemsServidos,
                a.ItemsCancelados,
                a.Cuentas,
                a.VentasUSD,
                a.Cuentas == 0 ? 0m : Math.Round(a.VentasUSD / a.Cuentas, 2),
                Porcentaje(a.ItemsCancelados, a.ItemsTotales),
                a.ItemsConTiempo == 0 ? null : Math.Round((decimal)a.SegundosServicio / a.ItemsConTiempo, 0)))
            .OrderByDescending(m => m.VentasGeneradasUSD)
            .ThenBy(m => m.Nombre)
            .ToList();

        return new ReporteMesonerosDto(inicio, fin, mesoneros.Sum(m => m.VentasGeneradasUSD), mesoneros);
    }

    private static decimal Porcentaje(decimal parte, decimal total)
        => total <= 0m ? 0m : Math.Round(parte / total * 100m, 2);

    private (DateTime Inicio, DateTime Fin) NormalizarRango(DateTime? desde, DateTime? hasta)
    {
        var fin = hasta is { } h ? FinDeRango(h) : _reloj.UtcNow;
        var inicio = desde is { } d ? ComoUtc(d) : fin.Subtract(RangoPorDefecto);

        if (inicio > fin)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
        }

        if ((fin - inicio).TotalDays > DiasMaximosPeriodo)
        {
            throw new ArgumentException($"El periodo no puede superar {DiasMaximosPeriodo} días.");
        }

        return (inicio, fin);
    }

    /// <summary>Un día sin hora ("2026-10-13") incluye el día completo; PostgreSQL no admite fechas sin zona.</summary>
    private static DateTime FinDeRango(DateTime hasta)
        => hasta.Kind == DateTimeKind.Unspecified && hasta.TimeOfDay == TimeSpan.Zero
            ? DateTime.SpecifyKind(hasta.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : ComoUtc(hasta);

    private static DateTime ComoUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
    };
}
