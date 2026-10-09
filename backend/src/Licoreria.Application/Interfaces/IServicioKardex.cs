using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>Insumo requerido para servir una variante (según su receta).</summary>
public sealed record KardexInsumo(Guid VarianteId, decimal Cantidad);

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

    /// <summary>
    /// Desglosa la cantidad de una variante en sus insumos: si la variante tiene
    /// receta devuelve una fila por insumo multiplicada por la cantidad; si no,
    /// devuelve la propia variante.
    /// </summary>
    Task<IReadOnlyList<KardexInsumo>> DesglosarInsumosAsync(
        Guid varianteId,
        decimal cantidad,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica que la existencia actual cubra los insumos requeridos y lanza
    /// <see cref="ReglaNegocioException"/> con el producto y las cantidades si no.
    /// </summary>
    Task VerificarStockAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default);
}
