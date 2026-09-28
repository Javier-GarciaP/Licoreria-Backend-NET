using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default);
}
