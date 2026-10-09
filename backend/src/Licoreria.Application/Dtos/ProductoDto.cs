using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record ProductoVarianteDto(
    Guid Id,
    string Nombre,
    string Sku,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    Guid UnidadMedidaId,
    string UnidadMedidaNombre,
    bool Activo,
    bool EsBase,
    IReadOnlyList<string> CodigosBarras,
    decimal Cantidad,
    decimal CantidadReservada,
    decimal StockMinimo,
    decimal StockMaximo);

public sealed record ProductoDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    Guid CategoriaId,
    string CategoriaNombre,
    Guid? MarcaId,
    string? MarcaNombre,
    Guid? ImpuestoId,
    TipoProducto Tipo,
    AreaDestino AreaDestino,
    decimal? GradoAlcoholico,
    string? ImagenUrl,
    bool Activo,
    IReadOnlyList<ProductoVarianteDto> Variantes);

public sealed record VarianteCrearDto(
    Guid? Id,
    string Nombre,
    string Sku,
    Guid UnidadMedidaId,
    decimal PrecioCompraUSD,
    decimal PrecioVentaUSD,
    IReadOnlyList<string>? CodigosBarras = null,
    decimal? StockInicial = null,
    decimal? StockMinimo = null,
    decimal? StockMaximo = null,
    bool EsBase = false);

public sealed record ProductoCrearDto(
    string Nombre,
    string? Descripcion,
    Guid CategoriaId,
    Guid? MarcaId,
    Guid? ImpuestoId,
    TipoProducto Tipo,
    AreaDestino AreaDestino,
    decimal? GradoAlcoholico,
    string? ImagenUrl,
    IReadOnlyList<VarianteCrearDto> Variantes);

public sealed record ProductoEditarDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    Guid CategoriaId,
    Guid? MarcaId,
    Guid? ImpuestoId,
    TipoProducto Tipo,
    AreaDestino AreaDestino,
    decimal? GradoAlcoholico,
    string? ImagenUrl,
    bool Activo,
    IReadOnlyList<VarianteCrearDto> Variantes);

public sealed record RecetaDto(
    Guid Id,
    Guid VarianteInsumoId,
    string VarianteInsumoNombre,
    string Sku,
    decimal Cantidad);

public sealed record RecetaCrearDto(
    Guid VarianteInsumoId,
    decimal Cantidad);
