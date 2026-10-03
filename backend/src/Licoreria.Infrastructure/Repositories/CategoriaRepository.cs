using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(LicoreriaDbContext context) : base(context)
    {
    }

    public Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(
            c => c.Nombre == nombre && !c.IsDeleted && (excluirId == null || c.Id != excluirId),
            cancellationToken);
}
