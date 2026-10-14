using Licoreria.Application.Common;
using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de CRM: clientes, fidelidad y cuentas por cobrar.
/// </summary>
public interface IServicioCrm
{
    Task<ResultadoPaginado<ClienteDto>> ObtenerClientesAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task<ClienteDto?> ObtenerClienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDto> CrearClienteAsync(ClienteCrearDto dto, CancellationToken cancellationToken = default);
    Task<ClienteDto?> EditarClienteAsync(ClienteEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PuntosMovimientoDto>> ObtenerPuntosAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<ClienteDto?> AcumularPuntosAsync(Guid clienteId, PuntosOperacionDto dto, CancellationToken cancellationToken = default);
    Task<ClienteDto?> CanjearPuntosAsync(Guid clienteId, PuntosOperacionDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<CuentaPorCobrarDto>> ObtenerCuentasPorCobrarAsync(
        PaginacionRequest paginacion,
        Guid? clienteId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default);

    Task<CuentaPorCobrarDto> RegistrarCuentaPorCobrarAsync(CuentaPorCobrarCrearDto dto, CancellationToken cancellationToken = default);
    Task<CuentaPorCobrarDto?> RegistrarPagoCuentaPorCobrarAsync(Guid cuentaId, PagoCuentaPorCobrarDto dto, CancellationToken cancellationToken = default);
}
