using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de caja: apertura, movimientos, arqueo y cierre (Z).
/// </summary>
public interface IServicioCaja
{
    Task<IReadOnlyList<DenominacionDto>> ObtenerDenominacionesAsync(CancellationToken cancellationToken = default);

    Task<SesionCajaDto> AbrirSesionAsync(AbrirCajaDto dto, CancellationToken cancellationToken = default);

    Task<SesionCajaDto?> ObtenerSesionActivaAsync(CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<SesionCajaDto>> ObtenerSesionesAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default);

    Task<SesionCajaDto?> RegistrarMovimientoAsync(
        Guid sesionId,
        MovimientoCajaCrearDto dto,
        CancellationToken cancellationToken = default);

    Task<SesionCajaDto?> CerrarSesionAsync(
        Guid sesionId,
        CerrarCajaDto dto,
        CancellationToken cancellationToken = default);
}
