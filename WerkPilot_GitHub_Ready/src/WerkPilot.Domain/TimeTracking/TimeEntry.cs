using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.TimeTracking;

public sealed class TimeEntry : Entity
{
    private TimeEntry() { }

    public TimeEntry(
        Guid userId,
        Guid projectId,
        Guid? projectTaskId,
        string description,
        DateTimeOffset startedAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Benutzer erforderlich.", nameof(userId));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Projekt erforderlich.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Tätigkeitsbeschreibung erforderlich.", nameof(description));

        UserId = userId;
        ProjectId = projectId;
        ProjectTaskId = projectTaskId;
        Description = description.Trim();
        StartedAtUtc = startedAtUtc;
    }

    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? ProjectTaskId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public bool IsRunning => !EndedAtUtc.HasValue;
    public decimal DurationHours => decimal.Round(
        (decimal)((EndedAtUtc ?? DateTimeOffset.UtcNow) - StartedAtUtc).TotalHours,
        2,
        MidpointRounding.AwayFromZero);

    public void Stop(DateTimeOffset endedAtUtc)
    {
        if (EndedAtUtc.HasValue)
            throw new InvalidOperationException("Die Zeiterfassung wurde bereits beendet.");
        if (endedAtUtc <= StartedAtUtc)
            throw new ArgumentException("Das Ende muss nach dem Beginn liegen.", nameof(endedAtUtc));

        EndedAtUtc = endedAtUtc;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateManual(
        string description,
        DateTimeOffset startedAtUtc,
        DateTimeOffset endedAtUtc,
        Guid? projectTaskId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Tätigkeitsbeschreibung erforderlich.", nameof(description));
        if (endedAtUtc <= startedAtUtc)
            throw new ArgumentException("Das Ende muss nach dem Beginn liegen.", nameof(endedAtUtc));

        Description = description.Trim();
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        ProjectTaskId = projectTaskId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
