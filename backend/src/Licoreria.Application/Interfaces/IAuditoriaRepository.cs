using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IAuditoriaRepository
{
    Task AgregarAsync(AuditLog log, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<AuditLog>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
