namespace WerkPilot.Application.Work;

public sealed record MyWorkSummaryDto(
    string UserDisplayName,
    int OpenCount,
    int DueTodayCount,
    int OverdueCount,
    int UrgentCount,
    int CustomerFollowUpCount,
    int ProjectTaskCount,
    IReadOnlyList<MyWorkItemDto> Items);
