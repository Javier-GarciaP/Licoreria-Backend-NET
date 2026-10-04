using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>Registro de auditoría de acciones sensibles.</summary>
public class AuditLog : BaseEntity
{
    public Guid? UsuarioId { get; set; }
    public string? Usuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
    public string? Datos { get; set; }
    public string? Ip { get; set; }
}
