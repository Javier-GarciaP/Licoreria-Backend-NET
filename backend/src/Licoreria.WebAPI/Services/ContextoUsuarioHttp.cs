using System.Security.Claims;
using Licoreria.Application.Interfaces;

namespace Licoreria.WebAPI.Services;

/// <summary>
/// Implementación de <see cref="IContextoUsuario"/> que lee los claims del JWT
/// del <see cref="HttpContext"/> actual.
/// </summary>
public sealed class ContextoUsuarioHttp : IContextoUsuario
{
    private readonly IHttpContextAccessor _accessor;

    public ContextoUsuarioHttp(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UsuarioId
    {
        get
        {
            var usuario = _accessor.HttpContext?.User;
            var valor = usuario?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? usuario?.FindFirstValue("sub");

            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public string? Email
        => _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
           ?? _accessor.HttpContext?.User.FindFirstValue("email");

    public bool EstaAutenticado
        => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? Ip
        => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
