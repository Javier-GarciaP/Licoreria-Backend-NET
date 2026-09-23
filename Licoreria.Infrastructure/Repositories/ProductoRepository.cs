using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(LicoreriaDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Producto>> GetByCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
                       .Where(p => p.CategoriaId == categoriaId)
                       .ToListAsync(cancellationToken);

    public Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(p => p.CodigoBarras == codigoBarras, cancellationToken);
}
