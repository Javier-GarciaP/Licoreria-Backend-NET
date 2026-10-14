using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record UsuarioDto(
    Guid Id,
    string NombreCompleto,
    string Email,
    string Rol,
    bool Activo,
    DateTime CreatedAt);

public sealed record UsuarioCrearDto(
    string NombreCompleto,
    string Email,
    string Password,
    RolUsuario Rol,
    bool Activo = true);

public sealed record UsuarioEditarDto(
    Guid Id,
    string NombreCompleto,
    string Email,
    RolUsuario Rol,
    bool Activo);

public sealed record CambiarPasswordDto(
    string PasswordNueva);

public sealed record RolDto(
    string Nombre,
    string Descripcion,
    IReadOnlyList<string> Permisos);

public sealed record PermisoDto(
    string Clave,
    string Modulo,
    string Descripcion);
