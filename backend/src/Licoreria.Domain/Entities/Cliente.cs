using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Cliente del local con datos de contacto y fiscales.</summary>
public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Rif { get; set; }
    public string? Ci { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public int Puntos { get; private set; }
    public bool Activo { get; set; } = true;

    public ICollection<PuntosMovimiento> Movimientos { get; set; } = new List<PuntosMovimiento>();

    public void AcumularPuntos(int puntos)
    {
        if (puntos <= 0)
        {
            throw new InvalidOperationException("Los puntos a acumular deben ser mayores que cero.");
        }

        Puntos += puntos;
        MarcarModificado();
    }

    public void CanjearPuntos(int puntos)
    {
        if (puntos <= 0)
        {
            throw new InvalidOperationException("Los puntos a canjear deben ser mayores que cero.");
        }

        if (puntos > Puntos)
        {
            throw new InvalidOperationException("El cliente no tiene puntos suficientes.");
        }

        Puntos -= puntos;
        MarcarModificado();
    }
}

/// <summary>Movimiento de puntos de fidelidad de un cliente.</summary>
public class PuntosMovimiento : BaseEntity
{
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public TipoMovimientoPuntos Tipo { get; set; }
    public int Puntos { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? ReferenciaTipo { get; set; }
    public Guid? ReferenciaId { get; set; }
}

/// <summary>Crédito otorgado a un cliente con saldo y vencimiento.</summary>
public class CuentaPorCobrar : BaseEntity
{
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public decimal MontoUSD { get; set; }
    public decimal SaldoUSD { get; private set; }
    public DateTime Vencimiento { get; set; }
    public EstadoCuentaPorCobrar Estado { get; set; } = EstadoCuentaPorCobrar.Pendiente;

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
            Estado = EstadoCuentaPorCobrar.Pagada;
        }

        MarcarModificado();
    }

    public void Inicializar(decimal monto)
    {
        MontoUSD = monto;
        SaldoUSD = monto;
    }
}
