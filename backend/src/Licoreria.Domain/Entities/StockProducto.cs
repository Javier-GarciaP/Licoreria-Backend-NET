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

    public void AplicarMovimiento(decimal cantidad)
    {
        var nuevo = Cantidad + cantidad;
        if (nuevo < 0)
        {
            throw new InvalidOperationException("El stock no puede quedar en negativo.");
        }

        Cantidad = nuevo;
        MarcarModificado();
    }

    public void Reservar(decimal cantidad)
    {
        if (cantidad < 0 || CantidadReservada + cantidad > Cantidad)
        {
            throw new InvalidOperationException("No hay stock suficiente para reservar.");
        }

        CantidadReservada += cantidad;
        MarcarModificado();
    }

    public void LiberarReserva(decimal cantidad)
    {
        CantidadReservada = Math.Max(0, CantidadReservada - cantidad);
        MarcarModificado();
    }
}
