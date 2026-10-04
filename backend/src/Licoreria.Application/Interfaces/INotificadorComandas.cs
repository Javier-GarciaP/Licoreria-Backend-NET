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
}
