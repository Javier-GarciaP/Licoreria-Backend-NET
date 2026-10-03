using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class UnidadMedida : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;

    public ICollection<ProductoVariante> Variantes { get; set; } = new List<ProductoVariante>();
}
