using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Línea de una venta. Referencia la variante vendida y fija el precio al momento.
/// </summary>
public class DetalleVenta : BaseEntity
{
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
    public decimal DescuentoUSD { get; set; }
    public bool EsCortesia { get; set; }

    public decimal SubtotalUSD => EsCortesia
        ? 0m
        : Math.Max(0m, (Cantidad * PrecioUnitarioUSD) - DescuentoUSD);
}
