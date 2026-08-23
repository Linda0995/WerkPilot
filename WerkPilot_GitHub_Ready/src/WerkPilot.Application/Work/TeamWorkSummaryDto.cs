namespace WerkPilot.Application.Work;

public sealed record TeamWorkSummaryDto(
    int ActiveUserCount,
    int OpenCount,
    int DueTodayCount,
    int OverdueCount,
    int UrgentCount,
    IReadOnlyList<TeamWorkUserSummaryDto> Users);
