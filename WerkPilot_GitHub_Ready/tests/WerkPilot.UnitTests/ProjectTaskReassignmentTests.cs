using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class ProjectTaskReassignmentTests
{
    [Fact]
    public void OpenTask_CanBeReassigned()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var task = new ProjectTask(
            1,
            "Montage",
            first,
            "Linda",
            null);

        task.Reassign(second, "Stephan");

        Assert.Equal(second, task.AssignedUserId);
        Assert.Equal("Stephan", task.AssignedTo);
    }

    [Fact]
    public void CompletedTask_CannotBeReassigned()
    {
        var task = new ProjectTask(
            1,
            "Montage",
            Guid.NewGuid(),
            "Linda",
            null);

        task.SetStatus(ProjectTaskStatus.Completed);

        Assert.Throws<InvalidOperationException>(() =>
            task.Reassign(Guid.NewGuid(), "Stephan"));
    }
}
