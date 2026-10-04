using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly LicoreriaDbContext _context;

    public AuditoriaRepository(LicoreriaDbContext context) => _context = context;

    public async Task AgregarAsync(AuditLog log, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AddAsync(log, cancellationToken);

    public Task<ResultadoPaginado<AuditLog>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.AuditLogs.AsNoTracking().Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entidad))
        {
            consulta = consulta.Where(a => a.Entidad == entidad);
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
