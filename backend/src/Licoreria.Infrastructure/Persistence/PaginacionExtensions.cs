using Licoreria.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Persistence;

/// <summary>
/// Extensión de paginación para consultas EF Core.
/// </summary>
public static class PaginacionExtensions
{
    public static async Task<ResultadoPaginado<T>> PaginarAsync<T>(
        this IQueryable<T> consulta,
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
    {
        var page = paginacion.PaginaNormalizada;
        var pageSize = paginacion.TamanoNormalizado;

        var totalItems = await consulta.CountAsync(cancellationToken);
        var items = await consulta
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ResultadoPaginado<T>.Crear(items, page, pageSize, totalItems);
    }
}
