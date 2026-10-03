using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Pago aplicado a una venta. Varias filas permiten el pago mixto.
/// </summary>
public class Pago : BaseEntity
{
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public Guid MetodoPagoId { get; set; }
    public MetodoPago MetodoPago { get; set; } = null!;

    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public decimal Propina { get; set; }
}
