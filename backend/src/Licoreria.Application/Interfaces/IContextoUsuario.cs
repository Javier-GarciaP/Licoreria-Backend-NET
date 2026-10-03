namespace Licoreria.Application.Interfaces;

/// <summary>
/// Expone el usuario autenticado en la petición actual.
/// </summary>
public interface IContextoUsuario
{
    Guid? UsuarioId { get; }

    string? Email { get; }

    bool EstaAutenticado { get; }
}
