using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Ocupación de una mesa: se abre al sentar al cliente y se cierra al cobrar.
/// </summary>
public class SesionMesa : BaseEntity
{
    public string NombreMesa { get; set; } = string.Empty;
    public Guid? MesaId { get; set; }
    public DateTime AbiertaEn { get; set; } = DateTime.UtcNow;
    public DateTime? CerradaEn { get; set; }

    /// <summary>Nombre del cliente sentado en la mesa (anotado por el mesonero al abrir).</summary>
    public string? Cliente { get; set; }

    /// <summary>Anotaciones de la mesa (preferencias, pedidos especiales, etc.).</summary>
    public string? Notas { get; set; }

    public Cuenta? Cuenta { get; set; }
}
