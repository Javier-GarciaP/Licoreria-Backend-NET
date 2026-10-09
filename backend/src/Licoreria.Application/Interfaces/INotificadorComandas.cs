namespace Licoreria.Application.Interfaces;

/// <summary>
/// Puerto para notificar en tiempo real los cambios de comandas y mesas.
/// La implementación concreta (SignalR) vive en la capa WebAPI.
/// </summary>
public interface INotificadorComandas
{
    Task ComandaCreadaAsync(Guid cuentaId, Guid comandaId, string area, CancellationToken cancellationToken = default);

    Task ComandaActualizadaAsync(Guid cuentaId, Guid comandaId, string area, CancellationToken cancellationToken = default);

    Task ItemActualizadoAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        string estado,
        string area,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica el cambio de estado de una mesa (Libre, Ocupada, Reservada) para que
    /// los planos de todos los clientes conectados se refresquen en tiempo real.
    /// </summary>
    Task MesaActualizadaAsync(
        Guid mesaId,
        string estado,
        Guid? cuentaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Difunde un mensaje del chat del personal al grupo <c>staff</c>.
    /// </summary>
    Task MensajeStaffAsync(Guid autorId, string autorNombre, string rol, string mensaje, CancellationToken cancellationToken = default);

    /// <summary>Notifica que se abrió un turno (sesión de caja) para que los operadores habiliten la operación.</summary>
    Task TurnoAbiertoAsync(Guid sesionId, CancellationToken cancellationToken = default);

    /// <summary>Notifica que se cerró el turno y las mesas quedaron libres.</summary>
    Task TurnoCerradoAsync(Guid sesionId, CancellationToken cancellationToken = default);
}
