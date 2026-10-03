using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de cuentas de mesa, comandas y abonos.
/// </summary>
public interface IServicioCuentas
{
    Task<CuentaDto> AbrirMesaAsync(AbrirMesaDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<CuentaDto>> ObtenerCuentasAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        CancellationToken cancellationToken = default);

    Task<CuentaDto?> ObtenerCuentaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CuentaDto?> AgregarComandaAsync(Guid cuentaId, CrearComandaDto dto, CancellationToken cancellationToken = default);

    Task<CuentaDto?> RegistrarAbonoAsync(Guid cuentaId, RegistrarAbonoCuentaDto dto, CancellationToken cancellationToken = default);

    Task<CuentaDto?> CambiarEstadoItemAsync(
        Guid cuentaId,
        Guid comandaId,
        Guid detalleId,
        ActualizarEstadoItemDto dto,
        CancellationToken cancellationToken = default);

    Task<VentaDto?> CerrarCuentaAsync(Guid cuentaId, CerrarCuentaDto dto, CancellationToken cancellationToken = default);
}
