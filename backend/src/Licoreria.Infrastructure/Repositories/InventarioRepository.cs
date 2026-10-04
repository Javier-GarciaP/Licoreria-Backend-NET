using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class InventarioRepository : IInventarioRepository
{
    private readonly LicoreriaDbContext _context;

    public InventarioRepository(LicoreriaDbContext context) => _context = context;

    public Task<StockProducto?> ObtenerStockAsync(Guid varianteId, CancellationToken cancellationToken = default)
        => _context.StockProductos.FirstOrDefaultAsync(s => s.VarianteId == varianteId, cancellationToken);

    public Task<ProductoVariante?> ObtenerVarianteConRecetasAsync(Guid varianteId, CancellationToken cancellationToken = default)
        => _context.ProductoVariantes
            .Include(v => v.Producto).ThenInclude(p => p.Recetas).ThenInclude(r => r.VarianteInsumo)
            .FirstOrDefaultAsync(v => v.Id == varianteId, cancellationToken);

    public Task<ResultadoPaginado<StockProducto>> ObtenerStockPaginadoAsync(
        PaginacionRequest paginacion,
        bool soloBajoMinimo = false,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.StockProductos
            .Include(s => s.Variante).ThenInclude(v => v.Producto)
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        if (soloBajoMinimo)
        {
            consulta = consulta.Where(s => s.Cantidad <= s.StockMinimo);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            consulta = consulta.Where(s =>
                s.Variante.Sku.ToLower().Contains(filtro) ||
                s.Variante.Producto.Nombre.ToLower().Contains(filtro));
        }

        return consulta.OrderBy(s => s.Variante.Producto.Nombre).PaginarAsync(paginacion, cancellationToken);
    }

    public async Task AgregarStockAsync(StockProducto stock, CancellationToken cancellationToken = default)
        => await _context.StockProductos.AddAsync(stock, cancellationToken);

    public Task<ResultadoPaginado<MovimientoInventario>> ObtenerMovimientosPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? varianteId = null,
        TipoMovimientoInventario? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.MovimientosInventario
            .Include(m => m.Variante)
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (varianteId is not null)
        {
            consulta = consulta.Where(m => m.VarianteId == varianteId);
        }

        if (tipo is not null)
        {
            consulta = consulta.Where(m => m.Tipo == tipo);
        }

        if (desde is not null)
        {
            consulta = consulta.Where(m => m.CreatedAt >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(m => m.CreatedAt <= hasta);
        }

        return consulta.OrderByDescending(m => m.CreatedAt).PaginarAsync(paginacion, cancellationToken);
    }

    public async Task AgregarMovimientoAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default)
        => await _context.MovimientosInventario.AddAsync(movimiento, cancellationToken);

    public Task<ResultadoPaginado<Merma>> ObtenerMermasPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Mermas
            .Include(m => m.Movimiento).ThenInclude(mi => mi.Variante)
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (desde is not null)
        {
            consulta = consulta.Where(m => m.CreatedAt >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(m => m.CreatedAt <= hasta);
        }

        return consulta.OrderByDescending(m => m.CreatedAt).PaginarAsync(paginacion, cancellationToken);
    }

    public async Task<IReadOnlyList<Merma>> ObtenerMermasParaReporteAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
        => await _context.Mermas
            .Include(m => m.Movimiento)
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.CreatedAt >= desde && m.CreatedAt <= hasta)
            .ToListAsync(cancellationToken);

    public async Task AgregarMermaAsync(Merma merma, CancellationToken cancellationToken = default)
        => await _context.Mermas.AddAsync(merma, cancellationToken);

    public Task<ResultadoPaginado<TomaFisica>> ObtenerTomasPaginadoAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
        => _context.TomasFisicas
            .Include(t => t.Detalles).ThenInclude(d => d.Variante)
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.Fecha)
            .PaginarAsync(paginacion, cancellationToken);

    public Task<TomaFisica?> ObtenerTomaConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.TomasFisicas
            .Include(t => t.Detalles).ThenInclude(d => d.Variante)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

    public async Task AgregarTomaAsync(TomaFisica toma, CancellationToken cancellationToken = default)
        => await _context.TomasFisicas.AddAsync(toma, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
