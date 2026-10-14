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

    Task<IReadOnlyList<Receta>> ObtenerRecetasAsync(Guid varianteVendidaId, CancellationToken cancellationToken = default);

    Task<Receta?> ObtenerRecetaAsync(Guid varianteVendidaId, Guid recetaId, CancellationToken cancellationToken = default);

    Task AgregarRecetaAsync(Receta receta, CancellationToken cancellationToken = default);

    Task AgregarVarianteAsync(ProductoVariante variante, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un código de barras nuevo de una variante ya existente. Debe agregarse explícitamente: una entidad
    /// con clave asignada que solo cuelga de una colección rastreada se interpretaría como existente (modificada).
    /// </summary>
    Task AgregarCodigoBarrasAsync(CodigoBarras codigo, CancellationToken cancellationToken = default);

    void EliminarReceta(Receta receta);
}
