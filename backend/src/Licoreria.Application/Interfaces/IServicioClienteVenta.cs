using Licoreria.Domain.Entities;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Relación de una venta con su cliente: validación, puntos de fidelidad y crédito (fiado).
/// Solo prepara los cambios en el contexto: los confirma quien guarda la venta, para que
/// venta, puntos y cuenta por cobrar viajen en la misma transacción.
/// </summary>
public interface IServicioClienteVenta
{
    /// <summary>Devuelve el cliente si existe y está activo; 404 si no existe o está eliminado, 422 si está inactivo.</summary>
    Task<Cliente> ValidarClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suma al cliente de la venta ⌊(total − crédito) × puntos por USD⌋ puntos y registra el movimiento.
    /// Devuelve los puntos otorgados (0 si no hay cliente, la tasa es 0 o no alcanza un punto).
    /// </summary>
    Task<int> AcumularPorVentaAsync(Venta venta, decimal creditoUSD, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la cuenta por cobrar de la porción pagada con Crédito, en USD con la tasa de la venta y
    /// enlazada a ella. Devuelve el monto a crédito en USD. Exige cliente y un vencimiento posterior a la venta.
    /// </summary>
    Task<decimal> RegistrarCreditoAsync(
        Venta venta,
        IReadOnlyList<Pago> pagosCredito,
        DateTime? vencimiento,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resta los puntos proporcionales al monto devuelto, sin dejar el saldo del cliente negativo.
    /// Devuelve los puntos revertidos.
    /// </summary>
    Task<int> RevertirPorDevolucionAsync(Venta venta, decimal montoDevueltoUSD, CancellationToken cancellationToken = default);

    /// <summary>¿La venta tiene una cuenta por cobrar con saldo pendiente?</summary>
    Task<bool> TieneCuentaPendienteAsync(Guid ventaId, CancellationToken cancellationToken = default);
}
