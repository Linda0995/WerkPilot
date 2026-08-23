namespace WerkPilot.Application.Dashboard;

public sealed record DashboardDto(
    int OpenOfferCount,
    decimal OpenOfferVolumeNet,
    int ActiveProjectCount,
    int OpenProjectTaskCount,
    int DueTaskCount,
    int OverdueTaskCount,
    int OpenCrmFollowUpCount,
    int OverdueCrmFollowUpCount,
    int OpenCustomerFollowUpCount,
    int DueTodayCustomerFollowUpCount,
    int OverdueCustomerFollowUpCount,
    int UrgentCustomerFollowUpCount,
    IReadOnlyList<DashboardTaskItem> DueTasks,
    IReadOnlyList<DashboardCrmFollowUpItem> CrmFollowUps,
    IReadOnlyList<DashboardCustomerFollowUpItem> CustomerFollowUps,
    IReadOnlyList<DashboardActivityItem> RecentItems);
