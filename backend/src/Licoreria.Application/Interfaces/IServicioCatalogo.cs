using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso del catálogo: categorías, marcas, unidades, impuestos,
/// listas de precio, productos (con variantes) y recetas.
/// </summary>
public interface IServicioCatalogo
{
    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);
    Task<CategoriaDto?> ObtenerCategoriaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoriaDto> CrearCategoriaAsync(CategoriaCrearDto dto, CancellationToken cancellationToken = default);
    Task<CategoriaDto?> EditarCategoriaAsync(CategoriaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarCategoriaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MarcaDto>> ObtenerMarcasAsync(CancellationToken cancellationToken = default);
    Task<MarcaDto> CrearMarcaAsync(MarcaCrearDto dto, CancellationToken cancellationToken = default);
    Task<MarcaDto?> EditarMarcaAsync(MarcaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarMarcaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnidadMedidaDto>> ObtenerUnidadesAsync(CancellationToken cancellationToken = default);
    Task<UnidadMedidaDto> CrearUnidadAsync(UnidadMedidaCrearDto dto, CancellationToken cancellationToken = default);
    Task<UnidadMedidaDto?> EditarUnidadAsync(UnidadMedidaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarUnidadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpuestoDto>> ObtenerImpuestosAsync(CancellationToken cancellationToken = default);
    Task<ImpuestoDto> CrearImpuestoAsync(ImpuestoCrearDto dto, CancellationToken cancellationToken = default);
    Task<ImpuestoDto?> EditarImpuestoAsync(ImpuestoEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarImpuestoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ListaPrecioDto>> ObtenerListasPrecioAsync(CancellationToken cancellationToken = default);
    Task<ListaPrecioDto> CrearListaPrecioAsync(ListaPrecioCrearDto dto, CancellationToken cancellationToken = default);
    Task<ListaPrecioDto?> EditarListaPrecioAsync(ListaPrecioEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarListaPrecioAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<ProductoDto>> ObtenerProductosAsync(
        PaginacionRequest paginacion,
        Guid? categoriaId = null,
        string? busqueda = null,
        bool? activo = null,
        CancellationToken cancellationToken = default);

    Task<ProductoDto?> ObtenerProductoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductoDto> CrearProductoAsync(ProductoCrearDto dto, CancellationToken cancellationToken = default);
    Task<ProductoDto?> EditarProductoAsync(ProductoEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarProductoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecetaDto>> ObtenerRecetasAsync(Guid productoId, CancellationToken cancellationToken = default);
    Task<RecetaDto> AgregarRecetaAsync(Guid productoId, RecetaCrearDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarRecetaAsync(Guid productoId, Guid recetaId, CancellationToken cancellationToken = default);

    // Modificadores / extras por producto
    Task<IReadOnlyList<ModificadorDto>> ObtenerModificadoresAsync(CancellationToken cancellationToken = default);
    Task<ModificadorDto> CrearModificadorAsync(ModificadorCrearDto dto, CancellationToken cancellationToken = default);
    Task<ModificadorDto?> EditarModificadorAsync(ModificadorEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarModificadorAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductoModificadorDto>> ObtenerModificadoresProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
    Task<ProductoModificadorDto> AsignarModificadorAsync(Guid productoId, AsignarModificadorDto dto, CancellationToken cancellationToken = default);
    Task<bool> QuitarModificadorAsync(Guid productoId, Guid productoModificadorId, CancellationToken cancellationToken = default);
}
