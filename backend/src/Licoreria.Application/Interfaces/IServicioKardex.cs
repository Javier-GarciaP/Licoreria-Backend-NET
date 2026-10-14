using Licoreria.Domain.Common;
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
    /// Verifica que la existencia disponible (cantidad menos lo reservado) cubra los insumos
    /// requeridos y lanza <see cref="ExistenciaAgotadaException"/> con el producto y las cantidades si no.
    /// </summary>
    Task VerificarStockAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserva los insumos para una comanda (verifica la existencia disponible y la compromete).
    /// Lanza <see cref="ExistenciaAgotadaException"/> si algún insumo no alcanza; no reserva nada parcialmente
    /// porque el llamador revierte la transacción.
    /// </summary>
    Task ReservarAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Libera la reserva de los insumos (ítem cancelado, mesa desalojada, cierre de cuenta o de turno).
    /// Es tolerante: nunca libera más de lo que figura reservado.
    /// </summary>
    Task LiberarReservaAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sirve un ítem reservado: consume la reserva, descuenta la existencia y registra el movimiento
    /// de kardex. Si la reserva no alcanza (datos anteriores a la reserva), descuenta el resto de la existencia.
    /// </summary>
    Task<MovimientoInventario> ConsumirReservaAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo = null,
        Guid? referenciaId = null,
        string? motivo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Libera la reserva de los ítems de comanda que la conservan y los marca como no reservados.
    /// Es el único camino de liberación: cancelar un ítem, desalojar la mesa, cerrar la cuenta y cerrar el turno.
    /// </summary>
    Task LiberarReservasAsync(
        IEnumerable<ComandaDetalle> detalles,
        CancellationToken cancellationToken = default);
}
