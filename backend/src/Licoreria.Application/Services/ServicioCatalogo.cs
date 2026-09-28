using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Services;

public sealed class ServicioCatalogo : IServicioCatalogo
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ServicioCatalogo(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<ProductoDto>> ObtenerProductosAsync(CancellationToken cancellationToken = default)
    {
        var productos = await _productoRepository.GetAllAsync(cancellationToken);
        return productos.Select(MapearProducto).ToList();
    }

    public async Task<ProductoDto?> ObtenerProductoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(id, cancellationToken);
        return producto is null ? null : MapearProducto(producto);
    }

    public async Task<ProductoDto> CrearProductoAsync(ProductoCrearDto dto, CancellationToken cancellationToken = default)
    {
        var producto = new Producto(
            dto.Nombre,
            dto.Descripcion,
            dto.Sku,
            dto.CodigoBarras,
            dto.PrecioCompraUSD,
            dto.PrecioVentaUSD,
            dto.StockMinimo,
            dto.StockMaximo,
            dto.CategoriaId,
            dto.MarcaId,
            dto.UnidadMedidaId);

        if (dto.Stock > 0)
        {
            producto.ActualizarStock(dto.Stock);
        }

        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl))
        {
            producto.ActualizarDatos(dto.Nombre, dto.Descripcion, dto.CodigoBarras, dto.ImagenUrl);
        }

        await _productoRepository.AddAsync(producto, cancellationToken);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        return MapearProducto(producto);
    }

    public async Task<ProductoDto?> EditarProductoAsync(ProductoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (producto is null)
        {
            return null;
        }

        producto.ActualizarDatos(dto.Nombre, dto.Descripcion, dto.CodigoBarras, dto.ImagenUrl);
        producto.ActualizarPrecios(dto.PrecioCompraUSD, dto.PrecioVentaUSD);

        _productoRepository.Update(producto);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        return MapearProducto(producto);
    }

    public async Task<bool> EliminarProductoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(id, cancellationToken);
        if (producto is null)
        {
            return false;
        }

        producto.EliminarLogico();
        _productoRepository.Update(producto);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.GetAllAsync(cancellationToken);
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nombre, c.Descripcion)).ToList();
    }

    public async Task<CategoriaDto> CrearCategoriaAsync(CategoriaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        await _categoriaRepository.AddAsync(categoria, cancellationToken);
        await _categoriaRepository.SaveChangesAsync(cancellationToken);

        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.Descripcion);
    }

    public async Task<bool> EliminarCategoriaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id, cancellationToken);
        if (categoria is null)
        {
            return false;
        }

        categoria.EliminarLogico();
        _categoriaRepository.Update(categoria);
        await _categoriaRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ProductoDto MapearProducto(Producto p)
        => new(
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.Sku,
            p.CodigoBarras,
            p.PrecioCompraUSD,
            p.PrecioVentaUSD,
            p.Stock,
            p.StockMinimo,
            p.StockMaximo,
            p.CategoriaId,
            p.MarcaId,
            p.UnidadMedidaId,
            p.ImagenUrl,
            p.Activo);
}
