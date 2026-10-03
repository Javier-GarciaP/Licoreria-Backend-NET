using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Usuario>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(
        string email,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);
}
