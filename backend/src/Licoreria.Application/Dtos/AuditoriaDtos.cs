namespace Licoreria.Application.Dtos;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UsuarioId,
    string? Usuario,
    string Accion,
    string Entidad,
    Guid? EntidadId,
    string? Datos,
    string? Ip,
    DateTime Fecha);
