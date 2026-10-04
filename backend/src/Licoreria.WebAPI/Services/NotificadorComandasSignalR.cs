using Licoreria.Application.Interfaces;
using Licoreria.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Licoreria.WebAPI.Services;

/// <summary>
/// Implementación de <see cref="INotificadorComandas"/> que difunde los cambios
/// a los grupos del área (barra/cocina) y a los meseros.
/// </summary>
public sealed class NotificadorComandasSignalR : INotificadorComandas
{
    private const string GrupoMeseros = "meseros";

    private readonly IHubContext<ComandasHub> _hub;

    public NotificadorComandasSignalR(IHubContext<ComandasHub> hub) => _hub = hub;

    public Task ComandaCreadaAsync(Guid cuentaId, Guid comandaId, string area, CancellationToken cancellationToken = default)
    {
        var payload = new { cuentaId, comandaId, area };
        return EnviarAsync("comanda:creada", area, payload, cancellationToken);
    }

    public Task ComandaActualizadaAsync(Guid cuentaId, Guid comandaId, string area, CancellationToken cancellationToken = default)
    {
        var payload = new { cuentaId, comandaId, area };
        return EnviarAsync("comanda:actualizada", area, payload, cancellationToken);
    }

    public Task ItemActualizadoAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        string estado,
        string area,
        CancellationToken cancellationToken = default)
    {
        var payload = new { cuentaId, comandaId, detalleId, estado, area };
        return EnviarAsync("item:actualizado", area, payload, cancellationToken);
    }

    private Task EnviarAsync(string evento, string area, object payload, CancellationToken cancellationToken)
    {
        var grupoArea = area.ToLowerInvariant();

        return Task.WhenAll(
            _hub.Clients.Group(grupoArea).SendAsync(evento, payload, cancellationToken),
            _hub.Clients.Group(GrupoMeseros).SendAsync(evento, payload, cancellationToken));
    }
}
