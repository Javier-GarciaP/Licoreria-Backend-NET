using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Lista de precios (detal, mayorista, happy hour).
/// </summary>
public class ListaPrecio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsPredeterminada { get; set; }
    public bool Activo { get; set; } = true;
}
