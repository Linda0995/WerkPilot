namespace WerkPilot.Application.Auditing;

public interface IAuditTrail
{
    Task WriteAsync(
        string entityType,
        Guid entityId,
        string action,
        string description,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEvent>> GetForEntityAsync(
        string entityType,
        Guid entityId,
        int maximumCount = 50,
        CancellationToken cancellationToken = default);
}
