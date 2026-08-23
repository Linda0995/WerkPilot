using WerkPilot.Application.Crm;
using WerkPilot.Application.Notifications;
using WerkPilot.Domain.Crm;

namespace WerkPilot.UnitTests;

public sealed class CustomerFollowUpNotificationPolicyTests
{
    [Fact]
    public void UrgentTask_IsCritical()
    {
        var severity = CustomerFollowUpNotificationPolicy.DetermineSeverity(
            overdueDays: 0,
            dueToday: false,
            urgent: true,
            CustomerFollowUpPriority.Urgent);

        Assert.Equal(NotificationSeverity.Critical, severity);
    }

    [Fact]
    public void ThreeDaysOverdue_IsCritical()
    {
        var severity = CustomerFollowUpNotificationPolicy.DetermineSeverity(
            overdueDays: 3,
            dueToday: false,
            urgent: false,
            CustomerFollowUpPriority.Normal);

        Assert.Equal(NotificationSeverity.Critical, severity);
    }

    [Fact]
    public void DueToday_IsWarning()
    {
        var severity = CustomerFollowUpNotificationPolicy.DetermineSeverity(
            overdueDays: 0,
            dueToday: true,
            urgent: false,
            CustomerFollowUpPriority.Normal);

        Assert.Equal(NotificationSeverity.Warning, severity);
    }

    [Fact]
    public void UpcomingNormalTask_IsInformation()
    {
        var severity = CustomerFollowUpNotificationPolicy.DetermineSeverity(
            overdueDays: 0,
            dueToday: false,
            urgent: false,
            CustomerFollowUpPriority.Normal);

        Assert.Equal(NotificationSeverity.Information, severity);
    }

    [Fact]
    public void CompletedTask_DoesNotCreateNotification()
    {
        var followUp = new CustomerFollowUpDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "KD-0001",
            "Muster GmbH",
            "Kunde anrufen",
            null,
            DateTimeOffset.Now,
            CustomerFollowUpPriority.High,
            CustomerFollowUpStatus.Completed,
            null,
            "Linda",
            "Stephan",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "Erledigt",
            false);

        var result = CustomerFollowUpNotificationPolicy.Create(
            followUp,
            DateOnly.FromDateTime(DateTime.Today),
            new HashSet<string>());

        Assert.Null(result);
    }
}
