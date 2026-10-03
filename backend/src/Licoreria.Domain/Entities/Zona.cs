using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Zona del local (barra, mesas, juegos, pista, VIP).</summary>
public class Zona : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public TipoZona Tipo { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}

/// <summary>Mesa con capacidad, forma y posición dentro del plano.</summary>
public class Mesa : BaseEntity
{
    public Guid ZonaId { get; set; }
    public Zona Zona { get; set; } = null!;

    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Forma { get; set; } = "redonda";
    public decimal PosX { get; set; }
    public decimal PosY { get; set; }
    public decimal Ancho { get; set; } = 1m;
    public decimal Alto { get; set; } = 1m;
    public bool Activa { get; set; } = true;
}

/// <summary>Versión del plano del local.</summary>
public class Plano : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool Activo { get; set; } = true;

    public ICollection<PlanoElemento> Elementos { get; set; } = new List<PlanoElemento>();
}

/// <summary>Elemento dibujado dentro de un plano (mesa, barra, baño, etc.).</summary>
public class PlanoElemento : BaseEntity
{
    public Guid PlanoId { get; set; }
    public Plano Plano { get; set; } = null!;

    public Guid? ZonaId { get; set; }
    public Zona? Zona { get; set; }

    public string Tipo { get; set; } = "mesa";
    public string? Etiqueta { get; set; }
    public decimal PosX { get; set; }
    public decimal PosY { get; set; }
    public decimal Ancho { get; set; } = 1m;
    public decimal Alto { get; set; } = 1m;
    public decimal Rotacion { get; set; }
}
