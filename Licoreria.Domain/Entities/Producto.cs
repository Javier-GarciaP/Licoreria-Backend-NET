using System;
using System.Collections.Generic;
using System.Text;

namespace Licoreria.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
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
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
}
