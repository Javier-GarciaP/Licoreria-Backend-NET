using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevocarTodosDelUsuarioAsync(Guid usuarioId, DateTime ahora, CancellationToken cancellationToken = default);
}
