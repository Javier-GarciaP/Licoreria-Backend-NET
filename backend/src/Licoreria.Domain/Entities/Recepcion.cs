using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Recepción de mercancía de una orden de compra. Actualiza inventario y costos.
/// </summary>
public class Recepcion : BaseEntity
{
    public Guid OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
    public decimal TotalUSD { get; private set; }

    public ICollection<RecepcionDetalle> Detalles { get; set; } = new List<RecepcionDetalle>();

    public void RecalcularTotal() => TotalUSD = Detalles.Sum(d => d.Cantidad * d.CostoUnitarioUSD);
}

/// <summary>Línea de una recepción.</summary>
public class RecepcionDetalle : BaseEntity
{
    public Guid RecepcionId { get; set; }
    public Recepcion Recepcion { get; set; } = null!;

    public Guid OrdenCompraDetalleId { get; set; }
    public OrdenCompraDetalle OrdenCompraDetalle { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal CostoUnitarioUSD { get; set; }
}
