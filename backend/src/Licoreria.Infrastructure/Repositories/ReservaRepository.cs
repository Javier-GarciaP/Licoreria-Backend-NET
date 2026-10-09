using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly LicoreriaDbContext _context;

    public ReservaRepository(LicoreriaDbContext context) => _context = context;

    private IQueryable<Reserva> ConDetalle()
        => _context.Reservas
            .Include(r => r.Mesas).ThenInclude(rm => rm.Mesa).ThenInclude(m => m.Zona)
            .Include(r => r.Pagos).ThenInclude(p => p.MetodoPago);

    public Task<ResultadoPaginado<Reserva>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoReserva? estado = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ConDetalle().AsNoTracking().Where(r => !r.IsDeleted);

        if (desde is not null)
        {
            consulta = consulta.Where(r => r.FechaHora >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(r => r.FechaHora <= hasta);
        }

        if (estado is not null)
        {
            consulta = consulta.Where(r => r.Estado == estado);
        }

        return consulta.OrderBy(r => r.FechaHora).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<Reserva?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

    public async Task AgregarAsync(Reserva reserva, CancellationToken cancellationToken = default)
        => await _context.Reservas.AddAsync(reserva, cancellationToken);

    public Task<ReservaPago?> ObtenerPagoAsync(Guid reservaId, Guid pagoId, CancellationToken cancellationToken = default)
        => _context.ReservaPagos.FirstOrDefaultAsync(
            p => p.Id == pagoId && p.ReservaId == reservaId && !p.IsDeleted,
            cancellationToken);

    public async Task AgregarPagoAsync(ReservaPago pago, CancellationToken cancellationToken = default)
        => await _context.ReservaPagos.AddAsync(pago, cancellationToken);

    public async Task<IReadOnlyList<Guid>> ObtenerMesasReservadasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var mesas = await _context.ReservaMesas
            .AsNoTracking()
            .Where(rm => !rm.IsDeleted
                && !rm.Reserva.IsDeleted
                && rm.Reserva.FechaHora >= desde
                && rm.Reserva.FechaHora <= hasta
                && (rm.Reserva.Estado == EstadoReserva.Pendiente || rm.Reserva.Estado == EstadoReserva.Confirmada))
            .Select(rm => rm.MesaId)
            .ToListAsync(cancellationToken);

        return mesas.Distinct().ToList();
    }

    public async Task<IReadOnlyList<IntervaloReserva>> ObtenerIntervalosActivosAsync(
        IEnumerable<Guid> mesaIds,
        DateTime desde,
        DateTime hasta,
        int horas,
        CancellationToken cancellationToken = default)
    {
        var ids = mesaIds.ToList();
        var filas = await _context.ReservaMesas
            .AsNoTracking()
            .Where(rm => !rm.IsDeleted
                && !rm.Reserva.IsDeleted
                && ids.Contains(rm.MesaId)
                && rm.Reserva.FechaHora >= desde
                && rm.Reserva.FechaHora <= hasta
                && (rm.Reserva.Estado == EstadoReserva.Pendiente || rm.Reserva.Estado == EstadoReserva.Confirmada))
            .Select(rm => new { MesaId = rm.MesaId, FechaHora = rm.Reserva.FechaHora })
            .ToListAsync(cancellationToken);

        return filas
            .Select(f => IntervaloReserva.Presunto(f.MesaId, f.FechaHora, horas))
            .ToList();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
