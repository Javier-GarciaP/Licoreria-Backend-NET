using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Insumo que consume un producto preparado (cóctel, tobo). Al venderse,
/// se descuentan del inventario las variantes indicadas.
/// </summary>
public class Receta : BaseEntity
{
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public Guid VarianteInsumoId { get; set; }
    public ProductoVariante VarianteInsumo { get; set; } = null!;

    public decimal Cantidad { get; set; }
}
