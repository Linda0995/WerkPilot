namespace WerkPilot.Application.Auditing;

public sealed record AuditEvent(
    string EntityType,
    Guid EntityId,
    string Action,
    string Description,
    DateTimeOffset OccurredAtUtc);
