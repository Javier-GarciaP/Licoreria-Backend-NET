using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Mensaje del chat entre el personal (mesoneros, barra y cocina).
/// Se persiste para conservar el historial reciente.
/// </summary>
public class ChatMensaje : BaseEntity
{
    public Guid AutorId { get; set; }
    public string AutorNombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
