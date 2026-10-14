using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Registro y consulta de auditoría de acciones sensibles.
/// </summary>
public interface IServicioAuditoria
{
    /// <summary>
    /// Agrega un evento de negocio o de acceso al contexto SIN guardar: viaja con el
    /// <c>SaveChanges</c> del cambio que describe (misma transacción).
    /// </summary>
    void Registrar(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null);

    /// <summary>
    /// Registra un evento y lo guarda enseguida. Para eventos sin transacción de negocio
    /// (inicio de sesión, intento fallido) que deben persistir aunque la petición falle.
    /// </summary>
    Task RegistrarAsync(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra y guarda un evento de acceso (inicio/cierre de sesión, intento fallido) a nombre de un
    /// usuario explícito: en esos momentos la petición todavía no tiene un usuario autenticado.
    /// </summary>
    Task RegistrarAccesoAsync(
        string accion,
        Guid? usuarioId,
        string? correo,
        object? datos = null,
        CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<AuditLogDto>> ObtenerAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? accion = null,
        TipoAuditoria? tipo = null,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerEntidadesAsync(CancellationToken cancellationToken = default);
}
