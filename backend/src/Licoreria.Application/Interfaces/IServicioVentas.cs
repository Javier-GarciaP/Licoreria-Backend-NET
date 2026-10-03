using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de ventas, pagos, comprobantes y devoluciones.
/// </summary>
public interface IServicioVentas
{
    Task<IReadOnlyList<MetodoPagoDto>> ObtenerMetodosPagoAsync(CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<VentaDto>> ObtenerVentasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<VentaDto?> ObtenerVentaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VentaDto> RegistrarVentaAsync(RegistrarVentaDto dto, CancellationToken cancellationToken = default);

    Task<VentaDto?> RegistrarPagoAsync(Guid ventaId, RegistrarPagoVentaDto dto, CancellationToken cancellationToken = default);

    Task<DevolucionDto?> RegistrarDevolucionAsync(Guid ventaId, RegistrarDevolucionDto dto, CancellationToken cancellationToken = default);
}
