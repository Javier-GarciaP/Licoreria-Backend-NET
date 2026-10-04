using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
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
}
