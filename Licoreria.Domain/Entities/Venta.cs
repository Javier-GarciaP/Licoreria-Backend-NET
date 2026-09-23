using System;
using System.Collections.Generic;
using System.Text;

namespace Licoreria.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal TasaCambio { get; set; }
    public decimal TotalUSD { get; set; }
    public decimal TotalBS { get; set; }
    public string MetodoPago { get; set; } = string.Empty; // EfectivoUSD, PagoMovil, Zelle, Punto

    // Relación N:1 con Usuario (Cajero)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Relación 1:N con DetalleVenta
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
