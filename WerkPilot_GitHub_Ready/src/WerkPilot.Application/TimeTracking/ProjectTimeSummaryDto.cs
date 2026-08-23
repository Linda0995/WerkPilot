namespace WerkPilot.Application.TimeTracking;

public sealed record ProjectTimeSummaryDto(
    Guid ProjectId,
    decimal TotalHours,
    decimal CompletedHours,
    decimal RunningHours,
    int EntryCount);
