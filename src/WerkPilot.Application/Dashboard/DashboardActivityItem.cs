namespace WerkPilot.Application.Dashboard;

public sealed record DashboardActivityItem(
    string Type,
    Guid EntityId,
    string Number,
    string Title,
    string Status,
    DateOnly ReferenceDate);
