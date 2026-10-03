using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Método de pago aceptado por el local.
/// </summary>
public class MetodoPago : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
