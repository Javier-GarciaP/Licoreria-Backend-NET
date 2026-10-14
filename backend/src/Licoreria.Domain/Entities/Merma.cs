using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Detalle de una merma: producto dañado, partido o vencido que se descuenta
/// del inventario y, opcionalmente, se repone sin cobro.
/// </summary>
public class Merma : BaseEntity
{
    public Guid MovimientoId { get; set; }
    public MovimientoInventario Movimiento { get; set; } = null!;

    public MotivoMerma Motivo { get; set; }
    public bool Repuesto { get; set; }
}
