using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Genera tokens JWT firmados para usuarios autenticados y tokens de refresco aleatorios.
/// </summary>
public interface ITokenService
{
    (string Token, DateTime Expira) GenerarToken(Usuario usuario);

    string GenerarTokenRefresco();
}
