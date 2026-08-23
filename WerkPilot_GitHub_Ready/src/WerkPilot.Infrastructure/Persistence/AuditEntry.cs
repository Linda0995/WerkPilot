namespace WerkPilot.Infrastructure.Persistence;

public sealed class AuditEntry
{
    private AuditEntry() { }

    public AuditEntry(
        string entityType,
        Guid entityId,
        string action,
        string description,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        Description = description;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private init; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
