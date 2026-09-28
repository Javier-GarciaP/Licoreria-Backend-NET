namespace Licoreria.Application.Dtos;

public sealed record LoginDto(
    string Username,
    string Password);

public sealed record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    string Role,
    DateTime Expira);
