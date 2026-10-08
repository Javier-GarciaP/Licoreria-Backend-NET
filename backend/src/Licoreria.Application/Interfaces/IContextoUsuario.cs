namespace Licoreria.Application.Interfaces;

/// <summary>
/// Expone el usuario autenticado en la petición actual.
/// </summary>
public interface IContextoUsuario
{
    Guid? UsuarioId { get; }

    string? Email { get; }

    string? RolDominio { get; }

    bool EstaAutenticado { get; }

    string? Ip { get; }
}
