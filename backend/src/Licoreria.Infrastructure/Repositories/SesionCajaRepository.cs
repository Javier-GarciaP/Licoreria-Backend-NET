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

    public async Task<SesionCaja?> ObtenerAbiertaBloqueadaAsync(CancellationToken cancellationToken = default)
    {
        var id = await _context.SesionesCaja
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Estado == EstadoSesionCaja.Abierta)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (id is not Guid sesionId)
        {
            return null;
        }

        await _context.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM sesiones_caja WHERE \"Id\" = {sesionId} FOR SHARE", cancellationToken);

        // Se vuelve a consultar tras obtener el bloqueo: si el turno se cerró mientras se esperaba, ya no figura abierto.
        return await _context.SesionesCaja.FirstOrDefaultAsync(
            s => s.Id == sesionId && !s.IsDeleted && s.Estado == EstadoSesionCaja.Abierta, cancellationToken);
    }

    public async Task BloquearParaCierreAsync(Guid sesionId, CancellationToken cancellationToken = default)
        => await _context.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM sesiones_caja WHERE \"Id\" = {sesionId} FOR UPDATE", cancellationToken);

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
        => ViolacionUnicidad.GuardarAsync(
            _context,
            cancellationToken,
            ("ux_sesiones_caja_abierta", "Ya existe una sesión de caja abierta."));
}
