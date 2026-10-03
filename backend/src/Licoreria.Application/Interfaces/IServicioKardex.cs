using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Aplica movimientos de kardex actualizando la existencia de la variante.
/// </summary>
public interface IServicioKardex
{
    Task<MovimientoInventario> AplicarAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo = null,
        Guid? referenciaId = null,
        string? motivo = null,
        CancellationToken cancellationToken = default);
}
