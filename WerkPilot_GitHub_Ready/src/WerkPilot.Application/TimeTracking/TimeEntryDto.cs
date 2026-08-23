namespace WerkPilot.Application.TimeTracking;

public sealed record TimeEntryDto(
    Guid Id,
    Guid UserId,
    Guid ProjectId,
    Guid? ProjectTaskId,
    string Description,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    bool IsRunning,
    decimal DurationHours);
