using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso del módulo de IA (trabajos y aprobación).
/// </summary>
public interface IServicioIa
{
    Task<AiGeneracionDto> SolicitarPlanoAsync(string nombreArchivo, CancellationToken cancellationToken = default);

    Task<AiGeneracionDto> SolicitarSeccionAsync(SolicitarSeccionIaDto dto, CancellationToken cancellationToken = default);

    Task<AiGeneracionDto> SolicitarImagenAsync(SolicitarImagenIaDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<AiGeneracionDto>> ObtenerGeneracionesAsync(
        PaginacionRequest paginacion,
        string? tipo = null,
        EstadoIa? estado = null,
        CancellationToken cancellationToken = default);

    Task<AiGeneracionDto?> ObtenerGeneracionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AiGeneracionDto?> AprobarAsync(Guid id, CancellationToken cancellationToken = default);
}
