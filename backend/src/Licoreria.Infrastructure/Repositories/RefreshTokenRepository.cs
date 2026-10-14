using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(LicoreriaDbContext context) : base(context)
    {
    }

    public Task<RefreshToken?> ObtenerPorTokenAsync(string token, CancellationToken cancellationToken = default)
        => _dbSet.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task RevocarTodosDelUsuarioAsync(
        Guid usuarioId,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(t => t.UsuarioId == usuarioId && !t.Revocado)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revocar(ahora);
        }
    }
}
