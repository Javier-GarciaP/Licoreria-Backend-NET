using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Impuesto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
