using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(LicoreriaDbContext context) : base(context)
    {
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking()
                 .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

    public Task<ResultadoPaginado<Usuario>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _dbSet.AsNoTracking().Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            consulta = consulta.Where(u =>
                u.NombreCompleto.ToLower().Contains(filtro) ||
                u.Email.ToLower().Contains(filtro));
        }

        return consulta
            .OrderBy(u => u.NombreCompleto)
            .PaginarAsync(paginacion, cancellationToken);
    }

    public Task<bool> ExisteEmailAsync(
        string email,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(
            u => u.Email == email && !u.IsDeleted && (excluirId == null || u.Id != excluirId),
            cancellationToken);
}
