using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class DetalleVenta : BaseEntity
{
    // Relación N:1 con Venta
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    // Relación N:1 con Producto
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
    public decimal SubtotalUSD => Cantidad * PrecioUnitarioUSD;
}
