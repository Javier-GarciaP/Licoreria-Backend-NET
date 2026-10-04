using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Lote de una variante con su fecha de vencimiento.</summary>
public class Lote : BaseEntity
{
    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public decimal Cantidad { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Toma física de inventario y sus diferencias.</summary>
public class TomaFisica : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public EstadoTomaFisica Estado { get; set; } = EstadoTomaFisica.Cerrada;
    public string? Observaciones { get; set; }

    public ICollection<TomaFisicaDetalle> Detalles { get; set; } = new List<TomaFisicaDetalle>();
}

/// <summary>Diferencia de una variante en una toma física.</summary>
public class TomaFisicaDetalle : BaseEntity
{
    public Guid TomaFisicaId { get; set; }
    public TomaFisica TomaFisica { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal CantidadSistema { get; set; }
    public decimal CantidadContada { get; set; }
    public decimal Diferencia => CantidadContada - CantidadSistema;
}
