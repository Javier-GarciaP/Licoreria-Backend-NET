using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// De qué presentación puede salir una porción y cuántas porciones da cada unidad: el vaso sale de
/// la botella de 1 L (4 vasos) o de la de 2 L (8 vasos). Las porciones por unidad son un entero.
/// </summary>
public class PorcionOrigen : BaseEntity
{
    public Guid PorcionVarianteId { get; set; }
    public ProductoVariante PorcionVariante { get; set; } = null!;

    public Guid PresentacionVarianteId { get; set; }
    public ProductoVariante PresentacionVariante { get; set; } = null!;

    public int PorcionesPorUnidad { get; set; }
}
