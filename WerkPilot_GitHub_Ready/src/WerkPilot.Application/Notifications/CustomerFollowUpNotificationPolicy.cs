using WerkPilot.Application.Crm;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Notifications;

public static class CustomerFollowUpNotificationPolicy
{
    public static NotificationItem? Create(
        CustomerFollowUpDto followUp,
        DateOnly today,
        IReadOnlySet<string> readKeys)
    {
        if (followUp.Status is CustomerFollowUpStatus.Completed
            or CustomerFollowUpStatus.Cancelled)
        {
            return null;
        }

        var dueDate = DateOnly.FromDateTime(
            followUp.DueAtUtc.ToLocalTime().DateTime);

        if (dueDate > today.AddDays(7))
            return null;

        var overdueDays = Math.Max(0, today.DayNumber - dueDate.DayNumber);
        var dueToday = dueDate == today;
        var urgent = followUp.Priority == CustomerFollowUpPriority.Urgent;

        var severity = DetermineSeverity(
            overdueDays,
            dueToday,
            urgent,
            followUp.Priority);

        var key =
            $"customer-follow-up:{followUp.Id}:{dueDate:yyyyMMdd}:{followUp.Priority}";

        var title = BuildTitle(
            followUp.CustomerName,
            overdueDays,
            dueToday,
            urgent);

        var description = BuildDescription(
            followUp,
            overdueDays);

        return new NotificationItem(
            key,
            severity,
            "Kunden-Aufgabe",
            title,
            description,
            dueDate,
            followUp.CustomerId,
            readKeys.Contains(key));
    }

    public static NotificationSeverity DetermineSeverity(
        int overdueDays,
        bool dueToday,
        bool urgent,
        CustomerFollowUpPriority priority)
    {
        if (overdueDays >= 3 || urgent)
            return NotificationSeverity.Critical;

        if (overdueDays > 0 || dueToday || priority == CustomerFollowUpPriority.High)
            return NotificationSeverity.Warning;

        return NotificationSeverity.Information;
    }

    private static string BuildTitle(
        string customerName,
        int overdueDays,
        bool dueToday,
        bool urgent)
    {
        if (overdueDays >= 3)
            return $"Kunden-Aufgabe für {customerName} ist seit {overdueDays} Tagen überfällig";

        if (overdueDays > 0)
            return $"Kunden-Aufgabe für {customerName} ist überfällig";

        if (urgent && dueToday)
            return $"Dringende Kunden-Aufgabe für {customerName} ist heute fällig";

        if (urgent)
            return $"Dringende Kunden-Aufgabe für {customerName} steht an";

        if (dueToday)
            return $"Kunden-Aufgabe für {customerName} ist heute fällig";

        return $"Kunden-Aufgabe für {customerName} wird fällig";
    }

    private static string BuildDescription(
        CustomerFollowUpDto followUp,
        int overdueDays)
    {
        var parts = new List<string>
        {
            followUp.Title,
            $"Priorität: {followUp.Priority}"
        };

        if (!string.IsNullOrWhiteSpace(followUp.AssignedTo))
            parts.Add($"Verantwortlich: {followUp.AssignedTo}");

        if (overdueDays > 0)
            parts.Add($"Überfällig: {overdueDays} Tag(e)");

        return string.Join(" · ", parts);
    }
}
