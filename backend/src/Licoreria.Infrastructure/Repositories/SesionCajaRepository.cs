using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class SesionCajaRepository : ISesionCajaRepository
{
    private readonly LicoreriaDbContext _context;

    public SesionCajaRepository(LicoreriaDbContext context) => _context = context;

    private IQueryable<SesionCaja> ConDetalle()
        => _context.SesionesCaja
            .Include(s => s.Movimientos)
            .Include(s => s.Arqueos).ThenInclude(a => a.Denominacion);

    public Task<SesionCaja?> ObtenerAbiertaAsync(CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(s => !s.IsDeleted && s.Estado == EstadoSesionCaja.Abierta, cancellationToken);

    public Task<SesionCaja?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

    public Task<ResultadoPaginado<SesionCaja>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
        => _context.SesionesCaja
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.AbiertaEn)
            .PaginarAsync(paginacion, cancellationToken);

    public async Task AgregarAsync(SesionCaja sesion, CancellationToken cancellationToken = default)
        => await _context.SesionesCaja.AddAsync(sesion, cancellationToken);

    public async Task AgregarMovimientoAsync(MovimientoCaja movimiento, CancellationToken cancellationToken = default)
        => await _context.MovimientosCaja.AddAsync(movimiento, cancellationToken);

    public async Task AgregarArqueoAsync(ArqueoDenominacion arqueo, CancellationToken cancellationToken = default)
        => await _context.ArqueosDenominacion.AddAsync(arqueo, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
