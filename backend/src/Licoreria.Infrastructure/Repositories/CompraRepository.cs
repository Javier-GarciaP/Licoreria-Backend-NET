using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class CompraRepository : ICompraRepository
{
    private readonly LicoreriaDbContext _context;

    public CompraRepository(LicoreriaDbContext context) => _context = context;

    private IQueryable<OrdenCompra> ConDetalle()
        => _context.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles).ThenInclude(d => d.Variante).ThenInclude(v => v.Producto);

    public Task<ResultadoPaginado<OrdenCompra>> ObtenerOrdenesPaginadoAsync(
        PaginacionRequest paginacion,
        EstadoOrdenCompra? estado = null,
        Guid? proveedorId = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ConDetalle().AsNoTracking().Where(o => !o.IsDeleted);

        if (estado is not null)
        {
            consulta = consulta.Where(o => o.Estado == estado);
        }

        if (proveedorId is not null)
        {
            consulta = consulta.Where(o => o.ProveedorId == proveedorId);
        }

        return consulta.OrderByDescending(o => o.Fecha).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<OrdenCompra?> ObtenerOrdenConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);

    public async Task AgregarOrdenAsync(OrdenCompra orden, CancellationToken cancellationToken = default)
        => await _context.OrdenesCompra.AddAsync(orden, cancellationToken);

    public Task<int> ContarOrdenesAsync(CancellationToken cancellationToken = default)
        => _context.OrdenesCompra.CountAsync(cancellationToken);

    private IQueryable<Recepcion> RecepcionesConDetalle()
        => _context.Recepciones
            .Include(r => r.OrdenCompra)
            .Include(r => r.Detalles).ThenInclude(d => d.Variante);

    public Task<ResultadoPaginado<Recepcion>> ObtenerRecepcionesPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? ordenCompraId = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = RecepcionesConDetalle().AsNoTracking().Where(r => !r.IsDeleted);

        if (ordenCompraId is not null)
        {
            consulta = consulta.Where(r => r.OrdenCompraId == ordenCompraId);
        }

        return consulta.OrderByDescending(r => r.Fecha).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<Recepcion?> ObtenerRecepcionConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => RecepcionesConDetalle().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

    public async Task AgregarRecepcionAsync(Recepcion recepcion, CancellationToken cancellationToken = default)
        => await _context.Recepciones.AddAsync(recepcion, cancellationToken);

    public Task<ResultadoPaginado<CuentaPorPagar>> ObtenerCuentasPorPagarPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? proveedorId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.CuentasPorPagar
            .Include(c => c.Proveedor)
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (proveedorId is not null)
        {
            consulta = consulta.Where(c => c.ProveedorId == proveedorId);
        }

        if (soloPendientes)
        {
            consulta = consulta.Where(c => c.Estado != EstadoCuentaPorPagar.Pagada);
        }

        return consulta.OrderBy(c => c.Vencimiento).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<CuentaPorPagar?> ObtenerCuentaPorPagarConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CuentasPorPagar
            .Include(c => c.Proveedor)
            .Include(c => c.Pagos)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

    public async Task AgregarCuentaPorPagarAsync(CuentaPorPagar cuenta, CancellationToken cancellationToken = default)
        => await _context.CuentasPorPagar.AddAsync(cuenta, cancellationToken);

    public async Task AgregarPagoProveedorAsync(PagoProveedor pago, CancellationToken cancellationToken = default)
        => await _context.PagosProveedor.AddAsync(pago, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
