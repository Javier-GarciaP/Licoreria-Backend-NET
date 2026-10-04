using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface ICompraRepository
{
    Task<ResultadoPaginado<OrdenCompra>> ObtenerOrdenesPaginadoAsync(
        PaginacionRequest paginacion,
        EstadoOrdenCompra? estado = null,
        Guid? proveedorId = null,
        CancellationToken cancellationToken = default);

    Task<OrdenCompra?> ObtenerOrdenConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarOrdenAsync(OrdenCompra orden, CancellationToken cancellationToken = default);

    Task<int> ContarOrdenesAsync(CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<Recepcion>> ObtenerRecepcionesPaginadoAsync(
        PaginacionRequest paginacion,
        Guid? ordenCompraId = null,
        CancellationToken cancellationToken = default);

    Task<Recepcion?> ObtenerRecepcionConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarRecepcionAsync(Recepcion recepcion, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
