using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Producto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;

    // Precios y Stock
    public decimal PrecioCompraUSD { get; set; }
    public decimal PrecioVentaUSD { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;

    // Imagen y Estado
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;

    // Relación N:1 con Categoria
    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
}
