using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Comprobante fiscal asociado a una venta (número de factura y de control).
/// </summary>
public class ComprobanteFiscal : BaseEntity
{
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string Numero { get; set; } = string.Empty;
    public string NumeroControl { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal TotalUSD { get; set; }
    public decimal TotalBS { get; set; }
}
