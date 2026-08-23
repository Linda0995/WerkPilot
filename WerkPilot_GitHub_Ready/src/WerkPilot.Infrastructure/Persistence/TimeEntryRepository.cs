using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.TimeTracking;
using WerkPilot.Domain.TimeTracking;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class TimeEntryRepository(WerkPilotDbContext dbContext)
    : ITimeEntryRepository
{
    public async Task<IReadOnlyList<TimeEntry>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken) =>
        await dbContext.TimeEntries
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.StartedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<TimeEntry?> GetRunningForUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.TimeEntries.SingleOrDefaultAsync(
            x => x.UserId == userId && x.EndedAtUtc == null,
            cancellationToken);

    public Task<TimeEntry?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.TimeEntries.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(
        TimeEntry entry,
        CancellationToken cancellationToken) =>
        dbContext.TimeEntries.AddAsync(entry, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
