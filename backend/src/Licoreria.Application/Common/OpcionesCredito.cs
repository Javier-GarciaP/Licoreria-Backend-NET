namespace Licoreria.Application.Common;

/// <summary>
/// Reglas de la venta a crédito (fiado). Se enlazan desde la sección "Credito" de la configuración.
/// </summary>
public sealed class OpcionesCredito
{
    public const string SectionName = "Credito";

    /// <summary>Días entre la venta y el vencimiento por defecto de la cuenta por cobrar.</summary>
    public int DiasVencimiento { get; set; } = 30;
}
