using WerkPilot.Domain.TimeTracking;

namespace WerkPilot.UnitTests;

public sealed class TimeEntryTests
{
    [Fact]
    public void Stop_CalculatesDuration()
    {
        var start = new DateTimeOffset(2026, 8, 2, 8, 0, 0, TimeSpan.Zero);
        var entry = new TimeEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Montage",
            start);

        entry.Stop(start.AddHours(2.5));

        Assert.False(entry.IsRunning);
        Assert.Equal(2.50m, entry.DurationHours);
    }

    [Fact]
    public void StopBeforeStart_Throws()
    {
        var start = DateTimeOffset.UtcNow;
        var entry = new TimeEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Montage",
            start);

        Assert.Throws<ArgumentException>(() =>
            entry.Stop(start.AddMinutes(-1)));
    }

    [Fact]
    public void ManualUpdate_ChangesTaskAndTimes()
    {
        var taskId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(-3);
        var entry = new TimeEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Alt",
            start);

        entry.Stop(start.AddHours(1));
        entry.UpdateManual(
            "Neu",
            start,
            start.AddHours(2),
            taskId);

        Assert.Equal("Neu", entry.Description);
        Assert.Equal(taskId, entry.ProjectTaskId);
        Assert.Equal(2m, entry.DurationHours);
    }
}
