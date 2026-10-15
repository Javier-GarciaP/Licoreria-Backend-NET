namespace Licoreria.Application.Common;

/// <summary>
/// Reglas de puntos de fidelidad. Se enlazan desde la sección "Fidelidad" de la configuración.
/// </summary>
public sealed class OpcionesFidelidad
{
    public const string SectionName = "Fidelidad";

    /// <summary>Puntos por cada USD pagado sin crédito; 0 desactiva la acumulación automática.</summary>
    public decimal PuntosPorUSD { get; set; } = 1m;
}
