namespace WerkPilot.Application.Dashboard;

public sealed record DashboardCrmFollowUpItem(
    Guid InteractionId,
    Guid CustomerId,
    string CustomerName,
    string Subject,
    string? FollowUpOwner,
    DateOnly FollowUpDate,
    bool IsOverdue);
