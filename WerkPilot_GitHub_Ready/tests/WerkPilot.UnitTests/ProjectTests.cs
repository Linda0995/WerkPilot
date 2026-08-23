using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class ProjectTests
{
    [Fact]
    public void AddTask_UpdatesOpenCountAndProgress()
    {
        var project = CreateProject();
        var first = project.AddTask("Konstruktion", null, "Max", null);
        project.AddTask("Fertigung", null, "Anna", null);

        project.UpdateTask(
            first.Id,
            first.Title,
            first.AssignedUserId,
            first.AssignedTo,
            first.DueDate,
            ProjectTaskStatus.Completed);

        Assert.Equal(1, project.OpenTaskCount);
        Assert.Equal(50, project.ProgressPercent);
    }

    [Fact]
    public void CompletedProject_RequiresAllTasksCompleted()
    {
        var project = CreateProject();
        project.AddTask("Offene Aufgabe", null, null, null);

        Assert.Throws<InvalidOperationException>(() =>
            project.SetStatus(ProjectStatus.Completed));
    }

    [Fact]
    public void CompletedProject_HasHundredPercentProgress()
    {
        var project = CreateProject();
        var task = project.AddTask("Aufgabe", null, null, null);

        project.UpdateTask(
            task.Id,
            task.Title,
            task.AssignedUserId,
            null,
            null,
            ProjectTaskStatus.Completed);

        project.SetStatus(ProjectStatus.Completed);

        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.Equal(100, project.ProgressPercent);
    }

    [Fact]
    public void RemoveTask_RenumbersRemainingTasks()
    {
        var project = CreateProject();
        var first = project.AddTask("A", null, null, null);
        project.AddTask("B", null, null, null);

        project.RemoveTask(first.Id);

        var remaining = Assert.Single(project.Tasks);
        Assert.Equal(1, remaining.PositionNumber);
    }

    [Fact]
    public void PlannedEndBeforeStart_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new Project(
                "PR-2026-0001",
                Guid.NewGuid(),
                null,
                "Test",
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 9)));
    }

    private static Project CreateProject() =>
        new(
            "PR-2026-0001",
            Guid.NewGuid(),
            null,
            "Testprojekt",
            new DateOnly(2026, 8, 2),
            null);
}
