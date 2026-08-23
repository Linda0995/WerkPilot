using WerkPilot.Domain.TimeTracking;

namespace WerkPilot.Application.TimeTracking;

public interface ITimeEntryRepository
{
    Task<IReadOnlyList<TimeEntry>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task<TimeEntry?> GetRunningForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<TimeEntry?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TimeEntry entry, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
