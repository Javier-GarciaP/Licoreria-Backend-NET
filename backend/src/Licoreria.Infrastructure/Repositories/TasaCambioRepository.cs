using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class TasaCambioRepository : ITasaCambioRepository
{
    private readonly LicoreriaDbContext _context;

    public TasaCambioRepository(LicoreriaDbContext context) => _context = context;

    public Task<TasaCambio?> ObtenerPorFechaAsync(DateTime fecha, TipoTasa tipo, CancellationToken cancellationToken = default)
        => _context.TasasCambio
            .FirstOrDefaultAsync(t => t.Fecha == fecha && t.Tipo == tipo && !t.IsDeleted, cancellationToken);

    public Task<TasaCambio?> ObtenerVigenteAsync(TipoTasa? tipo, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow.Date;
        var consulta = _context.TasasCambio
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.Fecha <= hoy);

        if (tipo is not null)
        {
            consulta = consulta.Where(t => t.Tipo == tipo);
        }

        return consulta
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TasaCambio>> ObtenerHistoricoAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        TipoTasa? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.TasasCambio.AsNoTracking().Where(t => !t.IsDeleted);

        if (desde is not null)
        {
            consulta = consulta.Where(t => t.Fecha >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(t => t.Fecha <= hasta);
        }

        if (tipo is not null)
        {
            consulta = consulta.Where(t => t.Tipo == tipo);
        }

        return await consulta.OrderByDescending(t => t.Fecha).ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(TasaCambio tasa, CancellationToken cancellationToken = default)
        => await _context.TasasCambio.AddAsync(tasa, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
