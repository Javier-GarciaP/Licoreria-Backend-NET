using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de inventario: existencias, kardex, mermas, cortesías y conversión de moneda.
/// </summary>
public interface IServicioInventario
{
    // Lógica de dominio (demostración)
    ResultadoMermaDto EvaluarMerma(MermaRequest request);
    decimal ConvertirAusdBolivares(decimal montoUsd, decimal tasaCambio);

    // Operación real
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
}
