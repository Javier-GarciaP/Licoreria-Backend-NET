namespace Licoreria.Application.Dtos;

public sealed record CategoriaDto(
    Guid Id,
    string Nombre,
    string Descripcion);

public sealed record CategoriaCrearDto(
    string Nombre,
    string Descripcion);

public sealed record CategoriaEditarDto(
    Guid Id,
    string Nombre,
    string Descripcion);
