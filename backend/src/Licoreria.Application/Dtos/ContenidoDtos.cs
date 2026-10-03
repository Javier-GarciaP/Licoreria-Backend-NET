namespace Licoreria.Application.Dtos;

public sealed record BloqueDto(
    Guid Id,
    string Tipo,
    int Orden,
    string Contenido,
    bool Activo);

public sealed record BloqueCrearDto(
    string Tipo,
    int Orden,
    string Contenido,
    bool Activo = true);

public sealed record SeccionDto(
    Guid Id,
    string Titulo,
    string Tipo,
    int Orden,
    bool Activa,
    IReadOnlyList<BloqueDto> Bloques);

public sealed record SeccionCrearDto(
    string Titulo,
    string Tipo,
    int Orden,
    bool Activa,
    IReadOnlyList<BloqueCrearDto> Bloques);

public sealed record PaginaDto(
    Guid Id,
    string Titulo,
    string Slug,
    bool Publicada,
    bool Activo,
    IReadOnlyList<SeccionDto> Secciones);

public sealed record PaginaCrearDto(
    string Titulo,
    string Slug,
    bool Publicada,
    IReadOnlyList<SeccionCrearDto> Secciones);

public sealed record PaginaEditarDto(
    Guid Id,
    string Titulo,
    string Slug,
    bool Publicada,
    bool Activo,
    IReadOnlyList<SeccionCrearDto> Secciones);

public sealed record HorarioDto(
    Guid Id,
    int DiaSemana,
    bool Abierto,
    TimeOnly? HoraApertura,
    TimeOnly? HoraCierre);

public sealed record HorarioGuardarDto(
    int DiaSemana,
    bool Abierto,
    TimeOnly? HoraApertura,
    TimeOnly? HoraCierre);

public sealed record LocalInfoDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    string Direccion,
    string Telefono,
    string Whatsapp,
    string Email,
    string? Instagram,
    string? Facebook,
    string? MapaUrl,
    string? LogoUrl);

public sealed record LocalInfoEditarDto(
    string Nombre,
    string Descripcion,
    string Direccion,
    string Telefono,
    string Whatsapp,
    string Email,
    string? Instagram,
    string? Facebook,
    string? MapaUrl,
    string? LogoUrl);

public sealed record MediaAssetDto(
    Guid Id,
    string Nombre,
    string Url,
    string Tipo,
    long Tamano);

public sealed record MenuItemDto(
    Guid VarianteId,
    string Nombre,
    string Sku,
    decimal PrecioUSD,
    decimal PrecioBS);

public sealed record MenuSeccionDto(
    Guid CategoriaId,
    string Nombre,
    IReadOnlyList<MenuItemDto> Items);

public sealed record MenuDigitalDto(
    string Nombre,
    decimal Tasa,
    DateTime GeneradoEn,
    IReadOnlyList<MenuSeccionDto> Secciones);

public sealed record QrMenuDto(
    string Url,
    string Contenido);
