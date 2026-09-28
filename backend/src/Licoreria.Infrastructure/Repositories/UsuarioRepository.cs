using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(LicoreriaDbContext context) : base(context)
    {
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking()
                 .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
}
