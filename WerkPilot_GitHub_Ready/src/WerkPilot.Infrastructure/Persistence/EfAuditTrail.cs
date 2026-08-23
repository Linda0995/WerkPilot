using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Auditing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class EfAuditTrail(WerkPilotDbContext dbContext) : IAuditTrail
{
    public async Task WriteAsync(
        string entityType,
        Guid entityId,
        string action,
        string description,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntry(
            entityType.Trim(),
            entityId,
            action.Trim(),
            description.Trim(),
            DateTimeOffset.UtcNow);

        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetForEntityAsync(
        string entityType,
        Guid entityId,
        int maximumCount = 50,
        CancellationToken cancellationToken = default)
    {
        var count = Math.Clamp(maximumCount, 1, 200);

        return await dbContext.AuditEntries
            .AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(count)
            .Select(x => new AuditEvent(
                x.EntityType,
                x.EntityId,
                x.Action,
                x.Description,
                x.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }
}
