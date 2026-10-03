using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class MovimientoTesoreriaRepository : IMovimientoTesoreriaRepository
{
    private readonly LicoreriaDbContext _context;

    public MovimientoTesoreriaRepository(LicoreriaDbContext context) => _context = context;

    public Task<ResultadoPaginado<MovimientoTesoreria>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        TipoMovimientoTesoreria? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.MovimientosTesoreria.AsNoTracking().Where(m => !m.IsDeleted);

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

    public async Task AgregarAsync(MovimientoTesoreria movimiento, CancellationToken cancellationToken = default)
        => await _context.MovimientosTesoreria.AddAsync(movimiento, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
