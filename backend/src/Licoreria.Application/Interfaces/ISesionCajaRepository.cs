using Licoreria.Application.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

public interface ISesionCajaRepository
{
    Task<SesionCaja?> ObtenerAbiertaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve el turno abierto tomando un bloqueo compartido de su fila durante la transacción en curso.
    /// Las operaciones que dependen del turno (ventas, comandas, movimientos) lo usan: el cierre de turno
    /// espera a que terminen y, una vez cerrado, esta consulta ya no lo devuelve (devuelve null).
    /// </summary>
    Task<SesionCaja?> ObtenerAbiertaBloqueadaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Toma un bloqueo exclusivo de la fila del turno durante la transacción en curso: el cierre espera a
    /// las operaciones en curso y serializa cierres simultáneos del mismo turno.
    /// </summary>
    Task BloquearParaCierreAsync(Guid sesionId, CancellationToken cancellationToken = default);

    Task<SesionCaja?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<SesionCaja>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(SesionCaja sesion, CancellationToken cancellationToken = default);

    Task AgregarMovimientoAsync(MovimientoCaja movimiento, CancellationToken cancellationToken = default);

    Task AgregarArqueoAsync(ArqueoDenominacion arqueo, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
