using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class PorcionRepository : IPorcionRepository
{
    private readonly LicoreriaDbContext _context;

    public PorcionRepository(LicoreriaDbContext context) => _context = context;

    public async Task<IReadOnlyList<PorcionOrigen>> ObtenerOrigenesAsync(Guid porcionVarianteId, CancellationToken cancellationToken = default)
        => await _context.PorcionOrigenes
            .Include(o => o.PresentacionVariante)
            .Where(o => !o.IsDeleted && o.PorcionVarianteId == porcionVarianteId)
            .OrderBy(o => o.PresentacionVariante.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PorcionOrigen>> ObtenerOrigenesDeVariantesAsync(IReadOnlyList<Guid> porcionIds, CancellationToken cancellationToken = default)
        => await _context.PorcionOrigenes
            .AsNoTracking()
            .Include(o => o.PresentacionVariante)
            .Where(o => !o.IsDeleted && porcionIds.Contains(o.PorcionVarianteId))
            .ToListAsync(cancellationToken);

    public async Task ReemplazarOrigenesAsync(Guid porcionVarianteId, IReadOnlyList<PorcionOrigen> nuevos, CancellationToken cancellationToken = default)
    {
        var actuales = await _context.PorcionOrigenes
            .Where(o => o.PorcionVarianteId == porcionVarianteId)
            .ToListAsync(cancellationToken);
        _context.PorcionOrigenes.RemoveRange(actuales);
        await _context.PorcionOrigenes.AddRangeAsync(nuevos, cancellationToken);
    }

    public Task<bool> TieneOrigenesAsync(Guid varianteId, CancellationToken cancellationToken = default)
        => _context.PorcionOrigenes.AnyAsync(o => !o.IsDeleted && o.PorcionVarianteId == varianteId, cancellationToken);

    public Task<bool> EsOrigenAsync(Guid presentacionId, CancellationToken cancellationToken = default)
        => _context.PorcionOrigenes.AnyAsync(o => !o.IsDeleted && o.PresentacionVarianteId == presentacionId, cancellationToken);

    public Task<bool> HayEnvasesAbiertosAsync(Guid varianteId, CancellationToken cancellationToken = default)
        => _context.EnvasesAbiertos.AnyAsync(
            e => !e.IsDeleted
                && e.Estado == EstadoEnvase.Abierto
                && (e.PorcionVarianteId == varianteId || e.PresentacionVarianteId == varianteId),
            cancellationToken);

    public async Task<IReadOnlySet<Guid>> ObtenerIdsConRecetaAsync(IReadOnlyList<Guid> varianteIds, CancellationToken cancellationToken = default)
        => (await _context.Recetas
            .AsNoTracking()
            .Where(r => !r.IsDeleted && varianteIds.Contains(r.VarianteVendidaId))
            .Select(r => r.VarianteVendidaId)
            .Distinct()
            .ToListAsync(cancellationToken)).ToHashSet();

    public async Task<IReadOnlyDictionary<Guid, ResumenPorcion>> ObtenerResumenAsync(IReadOnlyList<Guid> porcionIds, CancellationToken cancellationToken = default)
    {
        var abiertas = await _context.EnvasesAbiertos
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.Estado == EstadoEnvase.Abierto && porcionIds.Contains(e.PorcionVarianteId))
            .GroupBy(e => e.PorcionVarianteId)
            .Select(g => new { Id = g.Key, Porciones = g.Sum(e => e.PorcionesRestantes) })
            .ToDictionaryAsync(x => x.Id, x => x.Porciones, cancellationToken);

        var reservadas = await _context.StockPorciones
            .AsNoTracking()
            .Where(s => !s.IsDeleted && porcionIds.Contains(s.VarianteId))
            .ToDictionaryAsync(s => s.VarianteId, s => s.PorcionesReservadas, cancellationToken);

        return porcionIds.Distinct().ToDictionary(
            id => id,
            id => new ResumenPorcion(abiertas.GetValueOrDefault(id), reservadas.GetValueOrDefault(id)));
    }

    public async Task<IReadOnlyList<EnvaseAbierto>> ObtenerAbiertosAsync(Guid porcionVarianteId, CancellationToken cancellationToken = default)
        => await _context.EnvasesAbiertos
            .Where(e => !e.IsDeleted && e.Estado == EstadoEnvase.Abierto && e.PorcionVarianteId == porcionVarianteId)
            .OrderBy(e => e.AbiertoEn).ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

    public Task<EnvaseAbierto?> ObtenerEnvaseAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.EnvasesAbiertos
            .Include(e => e.PorcionVariante).ThenInclude(v => v.Producto)
            .Include(e => e.PresentacionVariante)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<EnvaseAbierto>> ListarAsync(
        Guid? porcionVarianteId,
        EstadoEnvase? estado,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.EnvasesAbiertos
            .AsNoTracking()
            .Include(e => e.PorcionVariante).ThenInclude(v => v.Producto)
            .Include(e => e.PresentacionVariante)
            .Where(e => !e.IsDeleted);

        if (porcionVarianteId is not null)
        {
            consulta = consulta.Where(e => e.PorcionVarianteId == porcionVarianteId);
        }

        if (estado is not null)
        {
            consulta = consulta.Where(e => e.Estado == estado);
        }

        // Abiertos: el más antiguo primero (es el que se agota antes). Historial: el más reciente primero.
        consulta = estado == EstadoEnvase.Abierto
            ? consulta.OrderBy(e => e.AbiertoEn)
            : consulta.OrderByDescending(e => e.AbiertoEn);

        return await consulta.Take(limite).ToListAsync(cancellationToken);
    }

    public async Task AgregarEnvaseAsync(EnvaseAbierto envase, CancellationToken cancellationToken = default)
        => await _context.EnvasesAbiertos.AddAsync(envase, cancellationToken);

    public Task<StockPorcion?> ObtenerStockPorcionAsync(Guid varianteId, CancellationToken cancellationToken = default)
        => _context.StockPorciones.FirstOrDefaultAsync(s => s.VarianteId == varianteId && !s.IsDeleted, cancellationToken);

    public async Task AgregarStockPorcionAsync(StockPorcion stock, CancellationToken cancellationToken = default)
        => await _context.StockPorciones.AddAsync(stock, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
