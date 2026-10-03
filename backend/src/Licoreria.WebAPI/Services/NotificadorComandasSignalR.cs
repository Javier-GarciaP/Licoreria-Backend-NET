using Licoreria.Application.Interfaces;
using Licoreria.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Licoreria.WebAPI.Services;

/// <summary>
/// Implementación de <see cref="INotificadorComandas"/> que difunde los cambios
/// de comandas a los clientes conectados mediante SignalR.
/// </summary>
public sealed class NotificadorComandasSignalR : INotificadorComandas
{
    private readonly IHubContext<ComandasHub> _hub;

    public NotificadorComandasSignalR(IHubContext<ComandasHub> hub) => _hub = hub;

    public Task ComandaCreadaAsync(Guid cuentaId, Guid comandaId, CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("comanda:creada", new { cuentaId, comandaId }, cancellationToken);

    public Task ComandaActualizadaAsync(Guid cuentaId, Guid comandaId, CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("comanda:actualizada", new { cuentaId, comandaId }, cancellationToken);

    public Task ItemActualizadoAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        string estado,
        CancellationToken cancellationToken = default)
        => _hub.Clients.All.SendAsync("item:actualizado", new { cuentaId, comandaId, detalleId, estado }, cancellationToken);
}
