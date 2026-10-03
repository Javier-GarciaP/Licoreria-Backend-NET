using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface IReservaRepository
{
    Task<ResultadoPaginado<Reserva>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoReserva? estado = null,
        CancellationToken cancellationToken = default);

    Task<Reserva?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Reserva reserva, CancellationToken cancellationToken = default);

    Task<ReservaPago?> ObtenerPagoAsync(Guid reservaId, Guid pagoId, CancellationToken cancellationToken = default);

    Task AgregarPagoAsync(ReservaPago pago, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
