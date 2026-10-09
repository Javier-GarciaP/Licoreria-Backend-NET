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
    private const string GrupoMesas = "mesas";

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

    public Task MesaActualizadaAsync(
        Guid mesaId,
        string estado,
        Guid? cuentaId = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new { mesaId, estado, cuentaId };

        return Task.WhenAll(
            _hub.Clients.Group(GrupoMeseros).SendAsync("mesa:actualizada", payload, cancellationToken),
            _hub.Clients.Group(GrupoMesas).SendAsync("mesa:actualizada", payload, cancellationToken));
    }

    public Task MensajeStaffAsync(
        Guid autorId,
        string autorNombre,
        string rol,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        var payload = new { autorId, autorNombre, rol, mensaje };
        return _hub.Clients.Group("staff").SendAsync("chat:recibido", payload, cancellationToken);
    }

    public Task TurnoAbiertoAsync(Guid sesionId, CancellationToken cancellationToken = default)
        => EnviarGlobalAsync("turno:abierto", new { sesionId }, cancellationToken);

    public Task TurnoCerradoAsync(Guid sesionId, CancellationToken cancellationToken = default)
        => EnviarGlobalAsync("turno:cerrado", new { sesionId }, cancellationToken);

    /// <summary>Difunde un evento a todas las áreas operativas (barra, cocina, meseros, mesas y staff).</summary>
    private Task EnviarGlobalAsync(string evento, object payload, CancellationToken cancellationToken)
        => Task.WhenAll(
            _hub.Clients.Group("barra").SendAsync(evento, payload, cancellationToken),
            _hub.Clients.Group("cocina").SendAsync(evento, payload, cancellationToken),
            _hub.Clients.Group("meseros").SendAsync(evento, payload, cancellationToken),
            _hub.Clients.Group("mesas").SendAsync(evento, payload, cancellationToken),
            _hub.Clients.Group("staff").SendAsync(evento, payload, cancellationToken));
}
