using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly LicoreriaDbContext _context;

    public VentaRepository(LicoreriaDbContext context) => _context = context;

    private IQueryable<Venta> ConDetalle()
        => _context.Ventas
            .Include(v => v.Detalles).ThenInclude(d => d.Variante).ThenInclude(va => va.Producto)
            .Include(v => v.Pagos).ThenInclude(p => p.MetodoPago)
            .Include(v => v.Comprobante);

    public Task<ResultadoPaginado<Venta>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoVenta? estado = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ConDetalle().AsNoTracking().Where(v => !v.IsDeleted);

        if (desde is not null)
        {
            consulta = consulta.Where(v => v.Fecha >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(v => v.Fecha <= hasta);
        }

        if (estado is not null)
        {
            consulta = consulta.Where(v => v.Estado == estado);
        }

        return consulta.OrderByDescending(v => v.Fecha).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<Venta?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);

    public async Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default)
        => await _context.Ventas.AddAsync(venta, cancellationToken);

    public async Task AgregarPagoAsync(Pago pago, CancellationToken cancellationToken = default)
        => await _context.Pagos.AddAsync(pago, cancellationToken);

    public Task<int> ContarComprobantesAsync(CancellationToken cancellationToken = default)
        => _context.ComprobantesFiscales.CountAsync(cancellationToken);

    public async Task AgregarDevolucionAsync(Devolucion devolucion, CancellationToken cancellationToken = default)
        => await _context.Devoluciones.AddAsync(devolucion, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
