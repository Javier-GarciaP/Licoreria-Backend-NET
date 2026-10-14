using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);
}
