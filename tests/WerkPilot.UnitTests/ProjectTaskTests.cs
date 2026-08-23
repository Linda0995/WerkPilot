using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class ProjectTaskTests
{
    [Fact]
    public void CompletingTask_SetsTimestamp()
    {
        var task = new ProjectTask(1, "Montage", null, "Max", new DateOnly(2026, 8, 10));

        task.SetStatus(ProjectTaskStatus.Completed);

        Assert.Equal(ProjectTaskStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAtUtc);
    }

    [Fact]
    public void ReopeningTask_RemovesCompletionTimestamp()
    {
        var task = new ProjectTask(1, "Montage", null, null, null);

        task.SetStatus(ProjectTaskStatus.Completed);
        task.SetStatus(ProjectTaskStatus.InProgress);

        Assert.Null(task.CompletedAtUtc);
    }
}
