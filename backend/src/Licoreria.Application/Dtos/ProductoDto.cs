namespace Licoreria.Application.Dtos;

public sealed record ProductoDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    string Sku,
    string CodigoBarras,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    int Stock,
    int StockMinimo,
    int StockMaximo,
    Guid CategoriaId,
    Guid MarcaId,
    Guid UnidadMedidaId,
    string? ImagenUrl,
    bool Activo);

public sealed record ProductoCrearDto(
    string Nombre,
    string? Descripcion,
    string Sku,
    string CodigoBarras,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    int Stock,
    int StockMinimo,
    int StockMaximo,
    Guid CategoriaId,
    Guid MarcaId,
    Guid UnidadMedidaId,
    string? ImagenUrl);

public sealed record ProductoEditarDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    string CodigoBarras,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    int StockMinimo,
    int StockMaximo,
    Guid CategoriaId,
    Guid MarcaId,
    Guid UnidadMedidaId,
    string? ImagenUrl);
