using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Existencia actual de una variante. La cantidad es la suma del kardex.
/// </summary>
public class StockProducto : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; private set; }
    public decimal CantidadReservada { get; private set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }

    public bool BajoMinimo => Cantidad <= StockMinimo;

    /// <summary>Existencia que se puede comprometer: la cantidad menos lo ya reservado para comandas.</summary>
    public decimal Disponible => Cantidad - CantidadReservada;

    public void AplicarMovimiento(decimal cantidad)
    {
        var nuevo = Cantidad + cantidad;
        if (nuevo < 0)
        {
            throw new InvalidOperationException("El stock no puede quedar en negativo.");
        }

        if (nuevo < CantidadReservada)
        {
            throw new InvalidOperationException(
                "El stock no puede quedar por debajo de lo reservado para comandas pendientes.");
        }

        Cantidad = nuevo;
        MarcarModificado();
    }

    /// <summary>Compromete existencia disponible para una comanda aún no servida.</summary>
    public void Reservar(decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad a reservar debe ser mayor que cero.");
        }

        if (cantidad > Disponible)
        {
            throw new InvalidOperationException("No hay stock suficiente para reservar.");
        }

        CantidadReservada += cantidad;
        MarcarModificado();
    }

    /// <summary>Devuelve a disponible lo reservado (ítem cancelado, mesa desalojada, turno cerrado).</summary>
    public void LiberarReserva(decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad a liberar debe ser mayor que cero.");
        }

        if (cantidad > CantidadReservada)
        {
            throw new InvalidOperationException("No hay tanto stock reservado para liberar.");
        }

        CantidadReservada -= cantidad;
        MarcarModificado();
    }

    /// <summary>Descuenta de la existencia lo reservado cuando el ítem se sirve.</summary>
    public void ConsumirReserva(decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad a consumir debe ser mayor que cero.");
        }

        if (cantidad > CantidadReservada)
        {
            throw new InvalidOperationException("No hay tanto stock reservado para consumir.");
        }

        CantidadReservada -= cantidad;
        Cantidad -= cantidad;
        MarcarModificado();
    }
}
