using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<IReadOnlyList<Producto>> ObtenerTodosConDetalleAsync(CancellationToken cancellationToken = default);

    Task<Producto?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Producto>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? categoriaId = null,
        string? busqueda = null,
        bool? activo = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteSkuAsync(
        string sku,
        Guid? excluirVarianteId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoBarrasAsync(
        string codigo,
        Guid? excluirVarianteId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Receta>> ObtenerRecetasAsync(Guid productoId, CancellationToken cancellationToken = default);

    Task<Receta?> ObtenerRecetaAsync(Guid productoId, Guid recetaId, CancellationToken cancellationToken = default);

    Task AgregarRecetaAsync(Receta receta, CancellationToken cancellationToken = default);

    Task AgregarVarianteAsync(ProductoVariante variante, CancellationToken cancellationToken = default);

    void EliminarReceta(Receta receta);
}
