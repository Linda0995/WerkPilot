using WerkPilot.Application.Dashboard;

namespace WerkPilot.UnitTests;

public sealed class DashboardModelTests
{
    [Fact]
    public void DashboardDto_PreservesOperationalCounters()
    {
        var dashboard = new DashboardDto(
            3,
            12000m,
            2,
            7,
            4,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            [],
            [],
            [],
            []);

        Assert.Equal(3, dashboard.OpenOfferCount);
        Assert.Equal(12000m, dashboard.OpenOfferVolumeNet);
        Assert.Equal(7, dashboard.OpenProjectTaskCount);
        Assert.Equal(1, dashboard.OverdueTaskCount);
    }

    [Fact]
    public void DashboardTaskItem_CanRepresentOverdueTask()
    {
        var item = new DashboardTaskItem(
            Guid.NewGuid(),
            "PR-2026-0001",
            "Geländer",
            Guid.NewGuid(),
            "Material bestellen",
            "Max",
            new DateOnly(2026, 8, 1),
            true);

        Assert.True(item.IsOverdue);
        Assert.Equal("Material bestellen", item.TaskTitle);
    }
}
