using WerkPilot.Domain.Identity;

namespace WerkPilot.Application.Work;

public sealed record TeamWorkUserSummaryDto(
    Guid UserId,
    string UserName,
    string DisplayName,
    UserRole Role,
    int OpenCount,
    int DueTodayCount,
    int OverdueCount,
    int UrgentCount,
    int CustomerFollowUpCount,
    int ProjectTaskCount,
    DateTimeOffset? NextDueAtUtc,
    IReadOnlyList<MyWorkItemDto> Items);
