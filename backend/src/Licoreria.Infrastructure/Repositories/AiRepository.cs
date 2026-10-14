using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class AiRepository : IAiRepository
{
    private readonly LicoreriaDbContext _context;

    public AiRepository(LicoreriaDbContext context) => _context = context;

    public Task<ResultadoPaginado<AiGeneracion>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? tipo = null,
        EstadoIa? estado = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.AiGeneraciones.AsNoTracking().Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            consulta = consulta.Where(a => a.Tipo == tipo);
        }

        if (estado is not null)
        {
            consulta = consulta.Where(a => a.Estado == estado);
        }

        return consulta.OrderByDescending(a => a.CreatedAt).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<AiGeneracion?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.AiGeneraciones.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

    public async Task AgregarAsync(AiGeneracion generacion, CancellationToken cancellationToken = default)
        => await _context.AiGeneraciones.AddAsync(generacion, cancellationToken);

    public async Task AgregarPlanoAsync(PlanoGenerado plano, CancellationToken cancellationToken = default)
        => await _context.PlanosGenerados.AddAsync(plano, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
