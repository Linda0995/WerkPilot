namespace WerkPilot.Application.Notifications;

public sealed record NotificationItem(
    string Key,
    NotificationSeverity Severity,
    string Category,
    string Title,
    string Description,
    DateOnly? DueDate,
    Guid ReferenceId,
    bool IsRead);
