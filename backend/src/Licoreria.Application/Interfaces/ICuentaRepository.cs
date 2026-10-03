using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface ICuentaRepository
{
    Task<ResultadoPaginado<Cuenta>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        CancellationToken cancellationToken = default);

    Task<Cuenta?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Cuenta cuenta, CancellationToken cancellationToken = default);

    Task AgregarComandaAsync(Comanda comanda, CancellationToken cancellationToken = default);

    Task AgregarAbonoAsync(Abono abono, CancellationToken cancellationToken = default);

    Task<ComandaDetalle?> ObtenerDetalleAsync(Guid comandaId, Guid detalleId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
