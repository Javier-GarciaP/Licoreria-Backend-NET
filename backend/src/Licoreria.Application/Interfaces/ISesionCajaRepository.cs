using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface ISesionCajaRepository
{
    Task<SesionCaja?> ObtenerAbiertaAsync(CancellationToken cancellationToken = default);

    Task<SesionCaja?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<SesionCaja>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(SesionCaja sesion, CancellationToken cancellationToken = default);

    Task AgregarMovimientoAsync(MovimientoCaja movimiento, CancellationToken cancellationToken = default);

    Task AgregarArqueoAsync(ArqueoDenominacion arqueo, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
