using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Cuenta de una mesa: acumula consumos y abonos hasta el cierre.
/// </summary>
public class Cuenta : BaseEntity
{
    public Guid SesionMesaId { get; set; }
    public SesionMesa SesionMesa { get; set; } = null!;

    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Abierta;
    public decimal Total { get; private set; }
    public decimal TotalAbonado { get; private set; }
    public decimal Saldo => Total - TotalAbonado;

    public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
    public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
    public ICollection<CuentaDivision> Divisiones { get; set; } = new List<CuentaDivision>();

    public void Acumular(decimal monto)
    {
        if (monto < 0)
        {
            throw new InvalidOperationException("El consumo no puede ser negativo.");
        }

        Total += monto;
        MarcarModificado();
    }

    /// <summary>
    /// Resta del total un ítem que dejó de formar parte de la cuenta (p. ej. cancelado).
    /// </summary>
    public void Descontar(decimal monto)
    {
        if (monto < 0)
        {
            throw new InvalidOperationException("El descuento no puede ser negativo.");
        }

        Total = Math.Max(0m, Total - monto);
        MarcarModificado();
    }

    /// <summary>
    /// Vuelve a poner la cuenta en estado Abierta para que el mesonero pueda
    /// seguir trabajándola (retomar una mesa que quedó por cobrar).
    /// </summary>
    public void Reabrir()
    {
        Estado = EstadoCuenta.Abierta;
        MarcarModificado();
    }

    public void Abonar(decimal monto)
    {
        if (monto <= 0)
        {
            throw new InvalidOperationException("El abono debe ser mayor que cero.");
        }

        TotalAbonado += monto;
        MarcarModificado();
    }

    public void Cerrar()
    {
        Estado = EstadoCuenta.Cerrada;
        MarcarModificado();
    }
}
