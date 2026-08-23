using WerkPilot.Application.Dashboard;

namespace WerkPilot.UnitTests;

public sealed class DashboardCrmFollowUpTests
{
    [Fact]
    public void Item_PreservesCustomerOwnerAndOverdueState()
    {
        var item = new DashboardCrmFollowUpItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Muster GmbH",
            "Angebot nachfassen",
            "Vertrieb",
            new DateOnly(2026, 8, 1),
            true);

        Assert.Equal("Muster GmbH", item.CustomerName);
        Assert.Equal("Vertrieb", item.FollowUpOwner);
        Assert.True(item.IsOverdue);
    }
}
