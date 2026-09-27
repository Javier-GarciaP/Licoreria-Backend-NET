using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    // Relación 1:N con Producto
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
