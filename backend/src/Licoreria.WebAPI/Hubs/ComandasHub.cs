using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Licoreria.WebAPI.Hubs;

/// <summary>
/// Hub en tiempo real para comandas y mesas. Los clientes se unen a su área
/// (barra, cocina, meseros) para recibir actualizaciones.
/// </summary>
[Authorize]
public class ComandasHub : Hub
{
    public Task UnirseArea(string area)
        => Groups.AddToGroupAsync(Context.ConnectionId, area);

    public Task SalirArea(string area)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, area);
}
