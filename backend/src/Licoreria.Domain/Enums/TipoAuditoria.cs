namespace Licoreria.Domain.Enums;

/// <summary>
/// Distingue un cambio de datos capturado automáticamente de un evento de negocio o de acceso.
/// </summary>
public enum TipoAuditoria
{
    Cambio = 1,
    Evento = 2
}
