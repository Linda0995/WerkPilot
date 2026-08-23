using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.TimeTracking;

namespace WerkPilot.Application.TimeTracking;

public sealed class TimeTrackingService(
    ITimeEntryRepository repository,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<TimeEntryDto>> GetForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        (await repository.GetForProjectAsync(projectId, cancellationToken))
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(Map)
            .ToArray();

    public async Task<TimeEntryDto?> GetRunningAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();
        var entry = await repository.GetRunningForUserAsync(userId, cancellationToken);
        return entry is null ? null : Map(entry);
    }

    public async Task<TimeEntryDto> StartAsync(
        Guid projectId,
        Guid? projectTaskId,
        string description,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();

        var running = await repository.GetRunningForUserAsync(userId, cancellationToken);
        if (running is not null)
            throw new InvalidOperationException(
                "Es läuft bereits eine Zeiterfassung. Diese muss zuerst beendet werden.");

        var entry = new TimeEntry(
            userId,
            projectId,
            projectTaskId,
            description,
            DateTimeOffset.UtcNow);

        await repository.AddAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "TimeEntry",
            entry.Id,
            "Started",
            $"Zeiterfassung für Projekt {projectId} wurde gestartet.",
            cancellationToken);

        return Map(entry);
    }

    public async Task<TimeEntryDto> StopAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();
        var entry = await repository.GetRunningForUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Es läuft keine Zeiterfassung.");

        entry.Stop(DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "TimeEntry",
            entry.Id,
            "Stopped",
            $"Zeiterfassung wurde nach {entry.DurationHours:N2} Stunden beendet.",
            cancellationToken);

        return Map(entry);
    }

    public async Task<TimeEntryDto> AddManualAsync(
        Guid projectId,
        Guid? projectTaskId,
        string description,
        DateTimeOffset startedAtUtc,
        DateTimeOffset endedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var entry = new TimeEntry(
            RequireUser(),
            projectId,
            projectTaskId,
            description,
            startedAtUtc);

        entry.Stop(endedAtUtc);

        await repository.AddAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(entry);
    }

    public async Task UpdateManualAsync(
        Guid entryId,
        string description,
        DateTimeOffset startedAtUtc,
        DateTimeOffset endedAtUtc,
        Guid? projectTaskId,
        CancellationToken cancellationToken = default)
    {
        var entry = await repository.GetAsync(entryId, cancellationToken)
            ?? throw new InvalidOperationException("Zeiteintrag wurde nicht gefunden.");

        if (entry.UserId != RequireUser())
            throw new UnauthorizedAccessException(
                "Zeiteinträge anderer Benutzer können nicht geändert werden.");

        entry.UpdateManual(
            description,
            startedAtUtc,
            endedAtUtc,
            projectTaskId);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjectTimeSummaryDto> GetProjectSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var entries = await repository.GetForProjectAsync(projectId, cancellationToken);

        return new ProjectTimeSummaryDto(
            projectId,
            entries.Sum(x => x.DurationHours),
            entries.Where(x => !x.IsRunning).Sum(x => x.DurationHours),
            entries.Where(x => x.IsRunning).Sum(x => x.DurationHours),
            entries.Count);
    }

    private Guid RequireUser() =>
        session.UserId
        ?? throw new InvalidOperationException("Keine aktive Benutzersitzung.");

    private static TimeEntryDto Map(TimeEntry x) => new(
        x.Id,
        x.UserId,
        x.ProjectId,
        x.ProjectTaskId,
        x.Description,
        x.StartedAtUtc,
        x.EndedAtUtc,
        x.IsRunning,
        x.DurationHours);
}
