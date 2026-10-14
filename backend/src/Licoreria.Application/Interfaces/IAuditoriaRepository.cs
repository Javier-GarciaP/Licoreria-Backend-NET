using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface IAuditoriaRepository
{
    /// <summary>Agrega la entrada al contexto sin guardar: se persiste con el próximo SaveChanges.</summary>
    void Agregar(AuditLog log);

    Task<ResultadoPaginado<AuditLog>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? accion = null,
        TipoAuditoria? tipo = null,
        CancellationToken cancellationToken = default);

    Task<AuditLog?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Entidades que tienen al menos una entrada, para poblar el filtro.</summary>
    Task<IReadOnlyList<string>> ObtenerEntidadesAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
