using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public Guid? CategoriaPadreId { get; set; }
    public Categoria? CategoriaPadre { get; set; }
    public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
