using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface IClienteRepository
{
    Task<ResultadoPaginado<Cliente>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default);

    Task<Cliente?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task AgregarMovimientoAsync(PuntosMovimiento movimiento, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<CuentaPorCobrar>> ObtenerCuentasPorCobrarAsync(
        PaginacionRequest paginacion,
        Guid? clienteId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default);

    Task<CuentaPorCobrar?> ObtenerCuentaPorCobrarAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarCuentaPorCobrarAsync(CuentaPorCobrar cuenta, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
