using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Dashboard;

public sealed record DashboardCustomerFollowUpItem(
    Guid FollowUpId,
    Guid CustomerId,
    string CustomerName,
    string Title,
    string? AssignedTo,
    DateTimeOffset DueAtUtc,
    CustomerFollowUpPriority Priority,
    bool IsOverdue,
    bool IsDueToday);
