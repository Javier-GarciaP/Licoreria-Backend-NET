namespace Licoreria.Application.Dtos;

public sealed record ProductoDto(
    Guid Id,
    string Nombre,
    string Sku,
    string CodigoBarras,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    int Stock,
    int StockMinimo,
    int StockMaximo,
    bool Activo);
