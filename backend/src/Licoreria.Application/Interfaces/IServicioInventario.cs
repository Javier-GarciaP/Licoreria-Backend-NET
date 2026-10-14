using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de inventario: existencias, kardex, mermas, cortesías.
/// </summary>
public interface IServicioInventario
{
    Task<ResultadoPaginado<StockDto>> ObtenerStockAsync(
        PaginacionRequest paginacion,
        bool soloBajoMinimo = false,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<MovimientoKardexDto>> ObtenerMovimientosAsync(
        PaginacionRequest paginacion,
        Guid? varianteId = null,
        TipoMovimientoInventario? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<MermaDto> RegistrarMermaAsync(RegistrarMermaDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<MermaDto>> ObtenerMermasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<MovimientoKardexDto> RegistrarAjusteAsync(AjusteInventarioDto dto, CancellationToken cancellationToken = default);

    Task<ReporteMermaDto> ObtenerReporteMermasAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoteDto>> ObtenerLotesAsync(Guid? varianteId = null, CancellationToken cancellationToken = default);
    Task<LoteDto> CrearLoteAsync(LoteCrearDto dto, CancellationToken cancellationToken = default);
    Task<LoteDto?> EditarLoteAsync(LoteEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarLoteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<TomaFisicaDto>> ObtenerTomasFisicasAsync(PaginacionRequest paginacion, CancellationToken cancellationToken = default);
    Task<TomaFisicaDto?> ObtenerTomaFisicaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TomaFisicaDto> RegistrarTomaFisicaAsync(RegistrarTomaFisicaDto dto, CancellationToken cancellationToken = default);
}
