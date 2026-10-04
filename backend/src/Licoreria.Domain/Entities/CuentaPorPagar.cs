using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Cuenta por pagar a un proveedor, generada al recibir una orden de compra.
/// </summary>
public class CuentaPorPagar : BaseEntity
{
    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public Guid? OrdenCompraId { get; set; }
    public Guid? RecepcionId { get; set; }

    public decimal MontoUSD { get; private set; }
    public decimal SaldoUSD { get; private set; }
    public DateTime Vencimiento { get; set; }
    public EstadoCuentaPorPagar Estado { get; set; } = EstadoCuentaPorPagar.Pendiente;

    public ICollection<PagoProveedor> Pagos { get; set; } = new List<PagoProveedor>();

    public void Inicializar(decimal monto)
    {
        MontoUSD = monto;
        SaldoUSD = monto;
    }

    public void RegistrarPago(decimal monto)
    {
        if (monto <= 0)
        {
            throw new InvalidOperationException("El pago debe ser mayor que cero.");
        }

        if (monto > SaldoUSD)
        {
            throw new InvalidOperationException("El pago no puede superar el saldo pendiente.");
        }

        SaldoUSD -= monto;
        if (SaldoUSD == 0)
        {
            Estado = EstadoCuentaPorPagar.Pagada;
        }

        MarcarModificado();
    }
}

/// <summary>Pago aplicado a una cuenta por pagar.</summary>
public class PagoProveedor : BaseEntity
{
    public Guid CuentaPorPagarId { get; set; }
    public CuentaPorPagar CuentaPorPagar { get; set; } = null!;

    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public Guid? MetodoPagoId { get; set; }
    public string? Referencia { get; set; }
}
