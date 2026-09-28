using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

public interface IServicioCatalogo
{
    Task<IReadOnlyList<ProductoDto>> ObtenerProductosAsync(CancellationToken cancellationToken = default);

    Task<ProductoDto?> ObtenerProductoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductoDto> CrearProductoAsync(ProductoCrearDto dto, CancellationToken cancellationToken = default);

    Task<ProductoDto?> EditarProductoAsync(ProductoEditarDto dto, CancellationToken cancellationToken = default);

    Task<bool> EliminarProductoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);

    Task<CategoriaDto> CrearCategoriaAsync(CategoriaCrearDto dto, CancellationToken cancellationToken = default);

    Task<bool> EliminarCategoriaAsync(Guid id, CancellationToken cancellationToken = default);
}
