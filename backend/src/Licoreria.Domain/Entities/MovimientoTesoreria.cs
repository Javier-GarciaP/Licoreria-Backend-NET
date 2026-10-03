using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Movimiento de tesorería (ingreso o egreso de fondos), independiente de la venta.
/// </summary>
public class MovimientoTesoreria : BaseEntity
{
    public TipoMovimientoTesoreria Tipo { get; set; }
    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public string Motivo { get; set; } = string.Empty;
    public string? ReferenciaTipo { get; set; }
    public Guid? ReferenciaId { get; set; }
}
