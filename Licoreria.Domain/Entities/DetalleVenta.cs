using System;
using System.Collections.Generic;
using System.Text;

namespace Licoreria.Domain.Entities;

public class DetalleVenta
{
    public int Id { get; set; }

    // Relación N:1 con Venta
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    // Relación N:1 con Producto
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
    public decimal SubtotalUSD => Cantidad * PrecioUnitarioUSD;
}
