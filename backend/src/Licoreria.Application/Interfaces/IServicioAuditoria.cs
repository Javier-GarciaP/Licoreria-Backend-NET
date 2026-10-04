using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Registro y consulta de auditoría de acciones sensibles.
/// </summary>
public interface IServicioAuditoria
{
    Task RegistrarAsync(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null,
        CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<AuditLogDto>> ObtenerAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);
}
