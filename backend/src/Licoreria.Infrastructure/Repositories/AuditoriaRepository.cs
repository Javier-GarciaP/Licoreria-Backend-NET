using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly LicoreriaDbContext _context;

    public AuditoriaRepository(LicoreriaDbContext context) => _context = context;

    public void Agregar(AuditLog log) => _context.AuditLogs.Add(log);

    public Task<ResultadoPaginado<AuditLog>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? accion = null,
        TipoAuditoria? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.AuditLogs.AsNoTracking().Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entidad))
        {
            consulta = consulta.Where(a => a.Entidad == entidad);
        }

        if (!string.IsNullOrWhiteSpace(accion))
        {
            consulta = consulta.Where(a => a.Accion == accion);
        }

        if (tipo is not null)
        {
            consulta = consulta.Where(a => a.Tipo == tipo);
        }

        if (usuarioId is not null)
        {
            consulta = consulta.Where(a => a.UsuarioId == usuarioId);
        }

        if (desde is not null)
        {
            consulta = consulta.Where(a => a.CreatedAt >= desde);
        }

        if (hasta is not null)
        {
            consulta = consulta.Where(a => a.CreatedAt <= hasta);
        }

        return consulta.OrderByDescending(a => a.CreatedAt).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<AuditLog?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<string>> ObtenerEntidadesAsync(CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Select(a => a.Entidad)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
