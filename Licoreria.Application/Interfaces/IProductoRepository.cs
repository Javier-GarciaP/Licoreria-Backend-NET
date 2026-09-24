using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<IReadOnlyList<Producto>> GetByCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default);
}
