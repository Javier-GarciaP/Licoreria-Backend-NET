namespace Licoreria.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;
}
