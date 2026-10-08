using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de finanzas: tasas de cambio.
/// </summary>
public interface IServicioFinanzas
{
    Task<TasaCambioDto?> ObtenerTasaActualAsync(TipoTasa tipo, CancellationToken cancellationToken = default);

    Task<decimal> ObtenerValorVigenteAsync(TipoTasa tipo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TasaCambioDto>> ObtenerHistoricoAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        TipoTasa? tipo = null,
        CancellationToken cancellationToken = default);

    Task<TasaCambioDto> RegistrarTasaAsync(RegistrarTasaDto dto, CancellationToken cancellationToken = default);
}