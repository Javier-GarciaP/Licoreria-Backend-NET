namespace Licoreria.Application.Dtos;

public sealed record ChatMensajeDto(
    Guid Id,
    Guid AutorId,
    string AutorNombre,
    string Rol,
    string Mensaje,
    DateTime CreadoEn);

public sealed record ChatEnviarDto(string Mensaje);
