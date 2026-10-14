using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Devolución de una venta con nota de crédito y, opcionalmente, reintegro de inventario.
/// </summary>
public class Devolucion : BaseEntity
{
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string Motivo { get; set; } = string.Empty;
    public decimal MontoUSD { get; set; }
    public bool ReintegrarInventario { get; set; }

    public ICollection<DevolucionDetalle> Detalles { get; set; } = new List<DevolucionDetalle>();
}

/// <summary>
/// Línea de una devolución.
/// </summary>
public class DevolucionDetalle : BaseEntity
{
    public Guid DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
}
