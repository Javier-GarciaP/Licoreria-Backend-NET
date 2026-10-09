using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Insumo que consume una variante vendida (presentación o preparado). Al
/// venderse la variante, se descuentan del inventario los insumos indicados.
/// </summary>
public class Receta : BaseEntity
{
    public Guid VarianteVendidaId { get; set; }
    public ProductoVariante VarianteVendida { get; set; } = null!;

    public Guid VarianteInsumoId { get; set; }
    public ProductoVariante VarianteInsumo { get; set; } = null!;

    public decimal Cantidad { get; set; }
}
