using WerkPilot.Domain.Crm;

namespace WerkPilot.UnitTests;

public sealed class CustomerFollowUpTests
{
    [Fact]
    public void OverdueOpenTask_IsReportedAsOverdue()
    {
        var task = CreateTask(DateTimeOffset.UtcNow.AddDays(-1));

        Assert.True(task.IsOverdue(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CompletedTask_IsNotOverdue()
    {
        var task = CreateTask(DateTimeOffset.UtcNow.AddDays(-1));
        task.Complete("Kunde erreicht.");

        Assert.False(task.IsOverdue(DateTimeOffset.UtcNow));
        Assert.Equal(CustomerFollowUpStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAtUtc);
    }

    [Fact]
    public void OpenTask_CanBeRescheduled()
    {
        var task = CreateTask(DateTimeOffset.UtcNow.AddDays(1));
        var newDue = DateTimeOffset.UtcNow.AddDays(5);

        task.Reschedule(
            newDue,
            CustomerFollowUpPriority.High,
            "Linda",
            null);

        Assert.Equal(newDue, task.DueAtUtc);
        Assert.Equal(CustomerFollowUpPriority.High, task.Priority);
        Assert.Equal("Linda", task.AssignedTo);
    }

    private static CustomerFollowUp CreateTask(DateTimeOffset dueAtUtc) =>
        new(
            Guid.NewGuid(),
            "KD-0001",
            "Muster GmbH",
            "Angebot nachfassen",
            "Telefonisch melden",
            dueAtUtc,
            CustomerFollowUpPriority.Normal,
            null,
            "Stephan",
            "Stephan");
}
