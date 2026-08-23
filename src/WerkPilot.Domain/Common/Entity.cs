namespace WerkPilot.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; protected init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void MoveToTrash()
    {
        IsDeleted = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
