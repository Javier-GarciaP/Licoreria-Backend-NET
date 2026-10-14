namespace Licoreria.Application.Dtos;

public sealed record CategoriaDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    Guid? CategoriaPadreId,
    bool Activo);

public sealed record CategoriaCrearDto(
    string Nombre,
    string Descripcion,
    Guid? CategoriaPadreId,
    bool Activo = true);

public sealed record CategoriaEditarDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    Guid? CategoriaPadreId,
    bool Activo);

public sealed record MarcaDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool Activo);

public sealed record MarcaCrearDto(
    string Nombre,
    string? Descripcion,
    bool Activo = true);

public sealed record MarcaEditarDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool Activo);

public sealed record UnidadMedidaDto(
    Guid Id,
    string Nombre,
    string Abreviatura);

public sealed record UnidadMedidaCrearDto(
    string Nombre,
    string Abreviatura);

public sealed record UnidadMedidaEditarDto(
    Guid Id,
    string Nombre,
    string Abreviatura);

public sealed record ImpuestoDto(
    Guid Id,
    string Nombre,
    decimal Porcentaje,
    bool Activo);

public sealed record ImpuestoCrearDto(
    string Nombre,
    decimal Porcentaje,
    bool Activo = true);

public sealed record ImpuestoEditarDto(
    Guid Id,
    string Nombre,
    decimal Porcentaje,
    bool Activo);

public sealed record ListaPrecioDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool EsPredeterminada,
    bool Activo);

public sealed record ListaPrecioCrearDto(
    string Nombre,
    string? Descripcion,
    bool EsPredeterminada = false,
    bool Activo = true);

public sealed record ListaPrecioEditarDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool EsPredeterminada,
    bool Activo);

public sealed record ModificadorDto(
    Guid Id,
    string Nombre,
    decimal PrecioAdicional,
    bool Activo);

public sealed record ModificadorCrearDto(
    string Nombre,
    decimal PrecioAdicional,
    bool Activo = true);

public sealed record ModificadorEditarDto(
    Guid Id,
    string Nombre,
    decimal PrecioAdicional,
    bool Activo);

public sealed record ProductoModificadorDto(
    Guid Id,
    Guid ModificadorId,
    string ModificadorNombre,
    decimal PrecioAdicional,
    int Minimo,
    int Maximo,
    bool Requerido);

public sealed record AsignarModificadorDto(
    Guid ModificadorId,
    int Minimo = 0,
    int Maximo = 1,
    bool Requerido = false);
