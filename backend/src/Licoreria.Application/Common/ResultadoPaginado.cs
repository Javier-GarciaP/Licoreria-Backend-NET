namespace Licoreria.Application.Common;

/// <summary>
/// Resultado paginado genérico que se expone al frontend.
/// </summary>
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages
        => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public static ResultadoPaginado<T> Crear(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
        => new(items, page, pageSize, totalItems);
}
