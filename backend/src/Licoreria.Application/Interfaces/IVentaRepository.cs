using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface IVentaRepository
{
    Task<ResultadoPaginado<Venta>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoVenta? estado = null,
        CancellationToken cancellationToken = default);

    Task<Venta?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default);

    Task<int> ContarComprobantesAsync(CancellationToken cancellationToken = default);

    Task AgregarDevolucionAsync(Devolucion devolucion, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
