using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de finanzas: monedas, tasas de cambio y tesorería.
/// </summary>
public interface IServicioFinanzas
{
    IReadOnlyList<MonedaDto> ObtenerMonedas();

    Task<TasaCambioDto?> ObtenerTasaActualAsync(TipoTasa tipo, CancellationToken cancellationToken = default);

    Task<decimal> ObtenerValorVigenteAsync(TipoTasa tipo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TasaCambioDto>> ObtenerHistoricoAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        TipoTasa? tipo = null,
        CancellationToken cancellationToken = default);

    Task<TasaCambioDto> RegistrarTasaAsync(RegistrarTasaDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<MovimientoTesoreriaDto>> ObtenerMovimientosAsync(
        PaginacionRequest paginacion,
        TipoMovimientoTesoreria? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<MovimientoTesoreriaDto> RegistrarMovimientoAsync(
        RegistrarMovimientoTesoreriaDto dto,
        CancellationToken cancellationToken = default);
}
