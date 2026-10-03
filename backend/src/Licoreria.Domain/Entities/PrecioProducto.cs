using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Precio de una variante en una lista y moneda determinadas.
/// </summary>
public class PrecioProducto : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public Guid ListaPrecioId { get; set; }
    public ListaPrecio ListaPrecio { get; set; } = null!;

    public Moneda Moneda { get; set; } = Moneda.USD;
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
}
