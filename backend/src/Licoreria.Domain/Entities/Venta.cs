using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Venta cerrada con totales en USD/BS y la tasa de cambio aplicada.
/// </summary>
public class Venta : BaseEntity
{
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal TasaCambio { get; set; }
    public decimal SubtotalUSD { get; set; }
    public decimal DescuentoUSD { get; set; }
    public decimal TotalUSD { get; set; }
    public decimal TotalBS { get; set; }
    public EstadoVenta Estado { get; set; } = EstadoVenta.Completada;

    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public Guid? CuentaId { get; set; }
    public Guid? PromocionId { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<Devolucion> Devoluciones { get; set; } = new List<Devolucion>();
    public ComprobanteFiscal? Comprobante { get; set; }

    public void Anular() => Estado = EstadoVenta.Anulada;
}
