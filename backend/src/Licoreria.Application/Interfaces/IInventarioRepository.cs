using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Acceso a existencias, kardex y mermas.
/// </summary>
public interface IInventarioRepository
{
    Task<StockProducto?> ObtenerStockAsync(Guid varianteId, CancellationToken cancellationToken = default);

    Task<ProductoVariante?> ObtenerVarianteConRecetasAsync(Guid varianteId, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<StockProducto>> ObtenerStockPaginadoAsync(
        PaginacionRequest paginacion,
        bool soloBajoMinimo = false,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task AgregarStockAsync(StockProducto stock, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<MovimientoInventario>> ObtenerMovimientosPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? varianteId = null,
        TipoMovimientoInventario? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task AgregarMovimientoAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Merma>> ObtenerMermasPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Merma>> ObtenerMermasParaReporteAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    Task AgregarMermaAsync(Merma merma, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<TomaFisica>> ObtenerTomasPaginadoAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default);

    Task<TomaFisica?> ObtenerTomaConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarTomaAsync(TomaFisica toma, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
