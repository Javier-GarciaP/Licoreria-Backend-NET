using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Método de pago aceptado por el local.
/// </summary>
public class MetodoPago : BaseEntity
{
    /// <summary>Código del método de pago «Crédito» (fiado): genera una cuenta por cobrar.</summary>
    public const string CodigoCredito = "Credito";

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
