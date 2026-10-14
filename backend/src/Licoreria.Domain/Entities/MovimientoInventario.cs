using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Movimiento inmutable del kardex de inventario. Una corrección se realiza
/// con un movimiento inverso, nunca sobrescribiendo el anterior.
/// </summary>
public class MovimientoInventario : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public TipoMovimientoInventario Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public string? ReferenciaTipo { get; set; }
    public Guid? ReferenciaId { get; set; }
    public string? Motivo { get; set; }
}
