namespace WerkPilot.Application.Dashboard;

public sealed record DashboardTaskItem(
    Guid ProjectId,
    string ProjectNumber,
    string ProjectTitle,
    Guid TaskId,
    string TaskTitle,
    string? AssignedTo,
    DateOnly? DueDate,
    bool IsOverdue);
