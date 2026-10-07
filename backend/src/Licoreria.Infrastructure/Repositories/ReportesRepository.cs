using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ReportesRepository : IReportesRepository
{
    private readonly LicoreriaDbContext _context;

    public ReportesRepository(LicoreriaDbContext context) => _context = context;

    public async Task<DashboardDto> ObtenerDashboardAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var ventasHoy = await _context.Ventas
            .Where(v => !v.IsDeleted && v.Estado == EstadoVenta.Completada && v.Fecha >= inicioDia)
            .GroupBy(v => 1)
            .Select(g => new { Total = g.Sum(v => v.TotalUSD), Cantidad = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        var ventasMes = await _context.Ventas
            .Where(v => !v.IsDeleted && v.Estado == EstadoVenta.Completada && v.Fecha >= inicioMes)
            .GroupBy(v => 1)
            .Select(g => new { Total = g.Sum(v => v.TotalUSD), Cantidad = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        var mermasMes = await _context.Mermas
            .Where(m => !m.IsDeleted && m.CreatedAt >= inicioMes)
            .GroupBy(m => 1)
            .Select(g => new { Cantidad = g.Count(), Unidades = g.Sum(m => m.Movimiento.Cantidad) })
            .FirstOrDefaultAsync(cancellationToken);

        var cuentasAbiertas = await _context.Cuentas
            .CountAsync(c => !c.IsDeleted && c.Estado != EstadoCuenta.Cerrada, cancellationToken);

        var stockBajo = await _context.StockProductos
            .CountAsync(s => !s.IsDeleted && s.Cantidad <= s.StockMinimo, cancellationToken);

        var limiteReservas = ahora.AddDays(7);
        var reservasProximas = await _context.Reservas
            .CountAsync(r => !r.IsDeleted
                && r.FechaHora >= ahora
                && r.FechaHora <= limiteReservas
                && (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada),
                cancellationToken);

        var caja = await _context.SesionesCaja
            .Where(s => !s.IsDeleted && s.Estado == EstadoSesionCaja.Abierta)
            .Select(s => (decimal?)s.FondoInicial)
            .FirstOrDefaultAsync(cancellationToken);

        var clientes = await _context.Clientes.CountAsync(c => !c.IsDeleted, cancellationToken);
        var productosActivos = await _context.Productos.CountAsync(p => !p.IsDeleted && p.Activo, cancellationToken);

        var totalMes = ventasMes?.Total ?? 0m;
        var cantidadMes = ventasMes?.Cantidad ?? 0;

        return new DashboardDto(
            ventasHoy?.Total ?? 0m,
            ventasHoy?.Cantidad ?? 0,
            totalMes,
            cantidadMes,
            cantidadMes == 0 ? 0m : Math.Round(totalMes / cantidadMes, 2),
            mermasMes?.Cantidad ?? 0,
            Math.Abs(mermasMes?.Unidades ?? 0m),
            cuentasAbiertas,
            stockBajo,
            reservasProximas,
            caja is not null,
            caja ?? 0m,
            clientes,
            productosActivos,
            ahora);
    }

    public async Task<ReportePropinasDto> ObtenerPropinasAsync(
        DateTime desde,
        DateTime hasta,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var filas = await _context.Pagos
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && p.Propina > 0
                && !p.Venta.IsDeleted
                && p.Venta.Estado == EstadoVenta.Completada
                && p.Venta.Fecha >= desde
                && p.Venta.Fecha <= hasta
                && (usuarioId == null || p.Venta.UsuarioId == usuarioId))
            .Select(p => new
            {
                p.Venta.UsuarioId,
                p.Venta.Usuario.NombreCompleto,
                p.VentaId,
                p.Propina
            })
            .ToListAsync(cancellationToken);

        var porUsuario = filas
            .GroupBy(f => new { f.UsuarioId, f.NombreCompleto })
            .Select(g => new PropinaPorUsuarioDto(
                g.Key.UsuarioId,
                g.Key.NombreCompleto,
                g.Select(x => x.VentaId).Distinct().Count(),
                g.Sum(x => x.Propina)))
            .OrderByDescending(p => p.TotalPropinaUSD)
            .ToList();

        return new ReportePropinasDto(desde, hasta, porUsuario.Sum(p => p.TotalPropinaUSD), porUsuario);
    }

    public async Task<ReporteVentasDto> ObtenerVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .Select(v => new
            {
                v.Fecha,
                v.TotalUSD,
                v.TotalBS,
                v.UsuarioId,
                v.Usuario.NombreCompleto
            })
            .ToListAsync(cancellationToken);

        var totalUsd = ventas.Sum(v => v.TotalUSD);
        var totalBs = ventas.Sum(v => v.TotalBS);

        var porDia = ventas
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new VentaPorDiaDto(g.Key, g.Sum(x => x.TotalUSD), g.Count()))
            .OrderBy(v => v.Fecha)
            .ToList();

        var porUsuario = ventas
            .GroupBy(v => new { v.UsuarioId, v.NombreCompleto })
            .Select(g => new VentaPorUsuarioDto(g.Key.UsuarioId, g.Key.NombreCompleto, g.Sum(x => x.TotalUSD), g.Count()))
            .OrderByDescending(v => v.TotalUSD)
            .ToList();

        var pagos = await _context.Pagos
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && !p.Venta.IsDeleted
                && p.Venta.Estado == EstadoVenta.Completada
                && p.Venta.Fecha >= desde
                && p.Venta.Fecha <= hasta)
            .Select(p => new
            {
                p.MetodoPagoId,
                p.MetodoPago.Nombre,
                p.Moneda,
                p.Monto
            })
            .ToListAsync(cancellationToken);

        var porMetodo = pagos
            .GroupBy(p => new { p.MetodoPagoId, p.Nombre, p.Moneda })
            .Select(g => new VentaPorMetodoPagoDto(
                g.Key.MetodoPagoId,
                g.Key.Nombre,
                g.Key.Moneda,
                g.Sum(x => x.Monto),
                g.Count()))
            .OrderByDescending(p => p.Monto)
            .ToList();

        return new ReporteVentasDto(
            desde,
            hasta,
            totalUsd,
            totalBs,
            ventas.Count,
            ventas.Count == 0 ? 0m : Math.Round(totalUsd / ventas.Count, 2),
            porDia,
            porUsuario,
            porMetodo);
    }

    public async Task<InventarioValorizadoDto> ObtenerInventarioValorizadoAsync(
        CancellationToken cancellationToken = default)
    {
        var filas = await (
            from s in _context.StockProductos.AsNoTracking()
            join v in _context.ProductoVariantes.AsNoTracking() on s.VarianteId equals v.Id
            where !s.IsDeleted && !v.IsDeleted
            select new
            {
                VarianteId = v.Id,
                v.Sku,
                ProductoNombre = v.Producto.Nombre,
                VarianteNombre = v.Nombre,
                s.Cantidad,
                s.StockMinimo,
                v.PrecioCompraUSD,
                v.PrecioVentaUSD
            })
            .ToListAsync(cancellationToken);

        var items = filas
            .Select(f => new InventarioValorizadoItemDto(
                f.VarianteId,
                f.Sku,
                f.ProductoNombre,
                f.VarianteNombre,
                f.Cantidad,
                f.PrecioCompraUSD,
                Math.Round(f.Cantidad * f.PrecioCompraUSD, 2),
                Math.Round(f.Cantidad * f.PrecioVentaUSD, 2),
                f.Cantidad <= f.StockMinimo))
            .OrderByDescending(i => i.CostoTotalUSD)
            .ToList();

        return new InventarioValorizadoDto(
            items.Count,
            items.Sum(i => i.Cantidad),
            items.Sum(i => i.CostoTotalUSD),
            items.Sum(i => i.ValorVentaUSD),
            items.Count(i => i.BajoMinimo),
            items);
    }

    public async Task<ReporteComprasDto> ObtenerComprasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var recepciones = await _context.Recepciones
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Fecha >= desde && r.Fecha <= hasta)
            .Select(r => r.TotalUSD)
            .ToListAsync(cancellationToken);

        var totalPorPagar = await _context.CuentasPorPagar
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Estado != EstadoCuentaPorPagar.Pagada)
            .SumAsync(c => (decimal?)c.SaldoUSD, cancellationToken) ?? 0m;

        var cuentasPendientes = await _context.CuentasPorPagar
            .CountAsync(c => !c.IsDeleted && c.Estado != EstadoCuentaPorPagar.Pagada, cancellationToken);

        var totalPagado = await _context.PagosProveedor
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && p.Moneda == Moneda.USD
                && p.CreatedAt >= desde
                && p.CreatedAt <= hasta)
            .SumAsync(p => (decimal?)p.Monto, cancellationToken) ?? 0m;

        return new ReporteComprasDto(
            desde,
            hasta,
            recepciones.Count,
            recepciones.Sum(),
            totalPorPagar,
            totalPagado,
            cuentasPendientes);
    }

    public async Task<ReporteHeatmapDto> ObtenerHeatmapAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .Select(v => new { v.Fecha, v.TotalUSD })
            .ToListAsync(cancellationToken);

        var franjas = ventas
            .GroupBy(v => new { Dia = (int)v.Fecha.DayOfWeek, v.Fecha.Hour })
            .Select(g => new HeatmapFranjaDto(
                g.Key.Dia,
                g.Key.Hour,
                g.Sum(x => x.TotalUSD),
                g.Count()))
            .OrderBy(f => f.DiaSemana)
            .ThenBy(f => f.Hora)
            .ToList();

        return new ReporteHeatmapDto(desde, hasta, franjas);
    }

    public async Task<IReadOnlyList<StockSaludCrudoDto>> ObtenerStockSaludAsync(
        CancellationToken cancellationToken = default)
    {
        return await (
            from s in _context.StockProductos.AsNoTracking()
            join v in _context.ProductoVariantes.AsNoTracking() on s.VarianteId equals v.Id
            where !s.IsDeleted && !v.IsDeleted
            select new StockSaludCrudoDto(
                v.Id,
                v.Sku,
                v.Producto.Nombre,
                v.Nombre,
                s.Cantidad,
                s.StockMinimo,
                s.StockMaximo))
            .ToListAsync(cancellationToken);
    }

    public async Task<(decimal VentasUSD, IReadOnlyList<ComparativoMermaDto> Mermas)> ObtenerMermasVsVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventasUsd = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .SumAsync(v => (decimal?)v.TotalUSD, cancellationToken) ?? 0m;

        var mermas = await _context.Mermas
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.CreatedAt >= desde && m.CreatedAt <= hasta)
            .Select(m => new
            {
                m.Motivo,
                Cantidad = m.Movimiento.Cantidad,
                Costo = m.Movimiento.CostoUnitario ?? m.Movimiento.Variante.PrecioCompraUSD
            })
            .ToListAsync(cancellationToken);

        var porMotivo = mermas
            .GroupBy(m => m.Motivo)
            .Select(g => new ComparativoMermaDto(
                g.Key,
                g.Count(),
                Math.Abs(g.Sum(x => x.Cantidad)),
                Math.Abs(g.Sum(x => x.Cantidad * x.Costo))))
            .OrderByDescending(m => m.ValorUSD)
            .ToList();

        return (ventasUsd, porMotivo);
    }
}
