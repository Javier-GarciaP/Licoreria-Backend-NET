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
    decimal StockMaximo,
    string Naturaleza = NaturalezaVariante.SinDefinir,
    int PorcionesDisponibles = 0,
    int PorcionesPotenciales = 0,
    int EnvasesCerradosAbribles = 0);

/// <summary>Cómo obtiene su existencia una variante.</summary>
public static class NaturalezaVariante
{
    /// <summary>Existencia propia: se compra al proveedor (botella 1 L, lata).</summary>
    public const string Presentacion = "Presentacion";

    /// <summary>Se sirve abriendo una presentación (vaso, trago).</summary>
    public const string Porcion = "Porcion";

    /// <summary>Consume insumos según una receta (preparados y recetas heredadas).</summary>
    public const string Receta = "Receta";

    /// <summary>Se vende sin inventario (productos de Cocina).</summary>
    public const string Directa = "Directa";

    public const string SinDefinir = "SinDefinir";
}

public sealed record OrigenPorcionDto(
    Guid Id,
    Guid PresentacionVarianteId,
    string PresentacionNombre,
    string Sku,
    int PorcionesPorUnidad);

public sealed record OrigenPorcionCrearDto(
    Guid PresentacionVarianteId,
    int PorcionesPorUnidad);

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
