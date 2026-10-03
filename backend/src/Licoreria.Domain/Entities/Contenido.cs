using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>Página de la web pública.</summary>
public class Pagina : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool Publicada { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Seccion> Secciones { get; set; } = new List<Seccion>();
}

/// <summary>Sección dentro de una página.</summary>
public class Seccion : BaseEntity
{
    public Guid PaginaId { get; set; }
    public Pagina Pagina { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "contenido";
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<BloqueContenido> Bloques { get; set; } = new List<BloqueContenido>();
}

/// <summary>Bloque de contenido (texto, galería, CTA).</summary>
public class BloqueContenido : BaseEntity
{
    public Guid SeccionId { get; set; }
    public Seccion Seccion { get; set; } = null!;

    public string Tipo { get; set; } = "texto";
    public int Orden { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>Archivo multimedia subido al servidor.</summary>
public class MediaAsset : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string RutaRelativa { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Tipo { get; set; } = "imagen";
    public long Tamano { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Horario de atención del local por día de la semana.</summary>
public class HorarioAtencion : BaseEntity
{
    public int DiaSemana { get; set; }
    public bool Abierto { get; set; } = true;
    public TimeOnly? HoraApertura { get; set; }
    public TimeOnly? HoraCierre { get; set; }
}

/// <summary>Información general del local para la web pública.</summary>
public class LocalInfo : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? MapaUrl { get; set; }
    public string? LogoUrl { get; set; }
}
