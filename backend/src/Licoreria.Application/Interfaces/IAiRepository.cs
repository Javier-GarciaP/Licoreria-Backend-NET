using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface IAiRepository
{
    Task<ResultadoPaginado<AiGeneracion>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? tipo = null,
        EstadoIa? estado = null,
        CancellationToken cancellationToken = default);

    Task<AiGeneracion?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(AiGeneracion generacion, CancellationToken cancellationToken = default);

    Task AgregarPlanoAsync(PlanoGenerado plano, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
