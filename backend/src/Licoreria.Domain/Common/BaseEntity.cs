namespace Licoreria.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;

    /// <summary>
    /// Registra la marca de tiempo UTC de la última modificación.
    /// Debe invocarse desde los métodos de dominio que alteran el estado.
    /// </summary>
    protected void MarcarModificado() => LastModifiedAt = DateTime.UtcNow;

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
