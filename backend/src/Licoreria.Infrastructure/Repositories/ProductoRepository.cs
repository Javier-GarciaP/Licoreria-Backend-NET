using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(LicoreriaDbContext context) : base(context)
    {
    }

    private IQueryable<Producto> ConDetalle()
        => _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Impuesto)
            .Include(p => p.Variantes).ThenInclude(v => v.UnidadMedida)
            .Include(p => p.Variantes).ThenInclude(v => v.CodigosBarras)
            .Include(p => p.Variantes).ThenInclude(v => v.Precios).ThenInclude(pr => pr.ListaPrecio);

    public async Task<IReadOnlyList<Producto>> ObtenerTodosConDetalleAsync(CancellationToken cancellationToken = default)
        => await ConDetalle().AsNoTracking().Where(p => !p.IsDeleted).ToListAsync(cancellationToken);

    public Task<Producto?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

    public Task<ResultadoPaginado<Producto>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? categoriaId = null,
        string? busqueda = null,
        bool? activo = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ConDetalle().AsNoTracking().Where(p => !p.IsDeleted);

        if (categoriaId is not null)
        {
            consulta = consulta.Where(p => p.CategoriaId == categoriaId);
        }

        if (activo is not null)
        {
            consulta = consulta.Where(p => p.Activo == activo);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.Nombre.ToLower().Contains(filtro) ||
                p.Variantes.Any(v => v.Sku.ToLower().Contains(filtro)));
        }

        return consulta.OrderBy(p => p.Nombre).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<bool> ExisteSkuAsync(
        string sku,
        Guid? excluirVarianteId = null,
        CancellationToken cancellationToken = default)
        => _context.Set<ProductoVariante>().AnyAsync(
            v => v.Sku == sku && (excluirVarianteId == null || v.Id != excluirVarianteId),
            cancellationToken);

    public Task<bool> ExisteCodigoBarrasAsync(
        string codigo,
        Guid? excluirVarianteId = null,
        CancellationToken cancellationToken = default)
        => _context.Set<CodigoBarras>().AnyAsync(
            c => c.Codigo == codigo && (excluirVarianteId == null || c.VarianteId != excluirVarianteId),
            cancellationToken);

    public async Task<IReadOnlyList<Receta>> ObtenerRecetasAsync(Guid productoId, CancellationToken cancellationToken = default)
        => await _context.Set<Receta>()
            .AsNoTracking()
            .Include(r => r.VarianteInsumo)
            .Where(r => r.ProductoId == productoId)
            .ToListAsync(cancellationToken);

    public Task<Receta?> ObtenerRecetaAsync(Guid productoId, Guid recetaId, CancellationToken cancellationToken = default)
        => _context.Set<Receta>()
            .FirstOrDefaultAsync(r => r.Id == recetaId && r.ProductoId == productoId, cancellationToken);

    public async Task AgregarRecetaAsync(Receta receta, CancellationToken cancellationToken = default)
        => await _context.Set<Receta>().AddAsync(receta, cancellationToken);

    public async Task AgregarVarianteAsync(ProductoVariante variante, CancellationToken cancellationToken = default)
        => await _context.Set<ProductoVariante>().AddAsync(variante, cancellationToken);

    public void EliminarReceta(Receta receta) => _context.Set<Receta>().Remove(receta);
}
