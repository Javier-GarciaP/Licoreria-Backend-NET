using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Genera tokens JWT firmados para usuarios autenticados.
/// </summary>
public interface ITokenService
{
    (string Token, DateTime Expira) GenerarToken(Usuario usuario);
}
