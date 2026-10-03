using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Código de barras asociado a una variante. Una variante puede tener varios.
/// </summary>
public class CodigoBarras : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public bool Principal { get; set; }
}
