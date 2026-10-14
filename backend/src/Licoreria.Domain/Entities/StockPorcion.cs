using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Porciones de una variante-porción comprometidas por comandas pendientes. Su fila serializa las
/// reservas de esa porción (token de versión): la disponibilidad es la suma de porciones abiertas
/// menos <see cref="PorcionesReservadas"/>.
/// </summary>
public class StockPorcion : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public int PorcionesReservadas { get; private set; }

    public void Reservar(int porciones)
    {
        if (porciones < 1)
        {
            throw new InvalidOperationException("Las porciones a reservar deben ser al menos una.");
        }

        PorcionesReservadas += porciones;
        MarcarModificado();
    }

    public void Liberar(int porciones)
    {
        if (porciones < 1)
        {
            throw new InvalidOperationException("Las porciones a liberar deben ser al menos una.");
        }

        if (porciones > PorcionesReservadas)
        {
            throw new InvalidOperationException("No hay tantas porciones reservadas para liberar.");
        }

        PorcionesReservadas -= porciones;
        MarcarModificado();
    }
}
