using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

public interface ITasaCambioRepository
{
    Task<TasaCambio?> ObtenerPorFechaAsync(DateTime fecha, TipoTasa tipo, CancellationToken cancellationToken = default);

    Task<TasaCambio?> ObtenerVigenteAsync(TipoTasa? tipo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TasaCambio>> ObtenerHistoricoAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        TipoTasa? tipo = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(TasaCambio tasa, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
