using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

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

    /// <summary>Mesas con reserva vigente (pendiente o confirmada) dentro de la ventana dada.</summary>
    Task<IReadOnlyList<Guid>> ObtenerMesasReservadasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intervalos presuntos de ocupación de las reservas vigentes (pendiente o confirmada)
    /// para las mesas dadas, con <see cref="Reserva.FechaHora"/> dentro de la ventana.
    /// </summary>
    Task<IReadOnlyList<IntervaloReserva>> ObtenerIntervalosActivosAsync(
        IEnumerable<Guid> mesaIds,
        DateTime desde,
        DateTime hasta,
        int horas,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializa la creación de reservas sobre las mismas mesas: toma, dentro de la transacción en curso,
    /// un bloqueo por mesa (en orden estable para evitar interbloqueos) que se libera al confirmar o
    /// revertir. Quien llega después espera y ve la reserva ya confirmada al comprobar los solapamientos.
    /// </summary>
    Task BloquearMesasAsync(IEnumerable<Guid> mesaIds, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
