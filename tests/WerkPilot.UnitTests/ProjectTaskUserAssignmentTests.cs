using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class ProjectTaskUserAssignmentTests
{
    [Fact]
    public void Task_PreservesAssignedUserId()
    {
        var userId = Guid.NewGuid();

        var task = new ProjectTask(
            1,
            "Montage vorbereiten",
            userId,
            "Linda",
            new DateOnly(2026, 8, 10));

        Assert.Equal(userId, task.AssignedUserId);
        Assert.Equal("Linda", task.AssignedTo);
    }

    [Fact]
    public void Update_CanChangeAssignedUser()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var task = new ProjectTask(
            1,
            "Montage",
            first,
            "Linda",
            null);

        task.Update(
            "Montage",
            second,
            "Stephan",
            null,
            ProjectTaskStatus.Open);

        Assert.Equal(second, task.AssignedUserId);
        Assert.Equal("Stephan", task.AssignedTo);
    }
}
