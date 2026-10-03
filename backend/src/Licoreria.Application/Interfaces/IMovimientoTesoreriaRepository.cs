using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface IMovimientoTesoreriaRepository
{
    Task<ResultadoPaginado<MovimientoTesoreria>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        TipoMovimientoTesoreria? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(MovimientoTesoreria movimiento, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
