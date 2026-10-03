namespace Licoreria.Application.Dtos;

public sealed record LoginDto(
    string Username,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record AuthResponseDto(
    Guid UsuarioId,
    string NombreCompleto,
    string Email,
    string Rol,
    IReadOnlyList<string> Permisos,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEn);

public sealed record UsuarioActualDto(
    Guid Id,
    string NombreCompleto,
    string Email,
    string Rol,
    IReadOnlyList<string> Permisos);
