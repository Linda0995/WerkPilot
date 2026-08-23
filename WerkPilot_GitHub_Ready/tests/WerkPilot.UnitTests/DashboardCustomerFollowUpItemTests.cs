using WerkPilot.Application.Dashboard;
using WerkPilot.Domain.Crm;

namespace WerkPilot.UnitTests;

public sealed class DashboardCustomerFollowUpItemTests
{
    [Fact]
    public void Item_PreservesUrgencyAndDueFlags()
    {
        var item = new DashboardCustomerFollowUpItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Muster GmbH",
            "Angebot nachfassen",
            "Linda",
            DateTimeOffset.UtcNow,
            CustomerFollowUpPriority.Urgent,
            true,
            true);

        Assert.Equal(CustomerFollowUpPriority.Urgent, item.Priority);
        Assert.True(item.IsOverdue);
        Assert.True(item.IsDueToday);
    }
}
