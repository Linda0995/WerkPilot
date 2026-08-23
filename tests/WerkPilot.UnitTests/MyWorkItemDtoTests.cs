using WerkPilot.Application.Work;

namespace WerkPilot.UnitTests;

public sealed class MyWorkItemDtoTests
{
    [Fact]
    public void TypeText_MapsCustomerFollowUp()
    {
        var item = new MyWorkItemDto(
            MyWorkItemType.CustomerFollowUp,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "KD-0001",
            "Muster GmbH",
            "Angebot nachfassen",
            "Linda",
            DateTimeOffset.UtcNow,
            "Urgent",
            "Open",
            true,
            false);

        Assert.Equal("Kunden-Aufgabe", item.TypeText);
    }

    [Fact]
    public void Summary_PreservesPersonalCounters()
    {
        var summary = new MyWorkSummaryDto(
            "Linda",
            6,
            2,
            1,
            1,
            4,
            2,
            []);

        Assert.Equal(6, summary.OpenCount);
        Assert.Equal(4, summary.CustomerFollowUpCount);
        Assert.Equal(2, summary.ProjectTaskCount);
    }
}
