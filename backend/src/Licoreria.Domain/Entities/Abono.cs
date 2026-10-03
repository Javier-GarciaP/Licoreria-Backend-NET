using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Abono parcial sobre una cuenta.
/// </summary>
public class Abono : BaseEntity
{
    public Guid CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public Guid MetodoPagoId { get; set; }
    public MetodoPago MetodoPago { get; set; } = null!;

    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
}
