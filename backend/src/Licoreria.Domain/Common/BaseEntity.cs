namespace Licoreria.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;

    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    /// <summary>
    /// Registra la marca de tiempo UTC de la última modificación.
    /// Debe invocarse desde los métodos de dominio que alteran el estado.
    /// </summary>
    protected void MarcarModificado() => LastModifiedAt = DateTime.UtcNow;

    /// <summary>
    /// Establece el usuario creador y la fecha de creación (usado por el interceptor de auditoría).
    /// </summary>
    public void EstablecerCreador(Guid? usuarioId, DateTime creadoEn)
    {
        CreatedBy = usuarioId;
        CreatedAt = creadoEn;
    }

    /// <summary>
    /// Establece el usuario modificador y la fecha de última modificación (usado por el interceptor de auditoría).
    /// </summary>
    public void EstablecerModificador(Guid? usuarioId, DateTime modificadoEn)
    {
        UpdatedBy = usuarioId;
        LastModifiedAt = modificadoEn;
    }

    /// <summary>
    /// Aplica borrado lógico: la entidad deja de exponerse sin eliminarse físicamente.
    /// </summary>
    public void EliminarLogico()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        MarcarModificado();
    }
}
