using WerkPilot.Domain.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryCountTests
{
    [Fact]
    public void FullyCountedInventory_BecomesReadyForPosting()
    {
        var count = new InventoryCount(
            "INV-2026-0001",
            "Jahresinventur",
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        count.AddLine(Guid.NewGuid(), 10m);
        count.StartCounting();

        var line = Assert.Single(count.Lines);
        count.RecordCount(line.Id, 9m, null, "Max");

        Assert.Equal(InventoryCountStatus.ReadyForPosting, count.Status);
        Assert.Equal(-1m, line.DifferenceQuantity);
        Assert.Equal(1m, count.AbsoluteDifferenceQuantity);
    }

    [Fact]
    public void PostingRequiresCompleteCount()
    {
        var count = new InventoryCount(
            "INV-2026-0001",
            "Jahresinventur",
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        count.AddLine(Guid.NewGuid(), 10m);
        count.StartCounting();

        Assert.Throws<InvalidOperationException>(() =>
            count.MarkPosted("Max"));
    }

    [Fact]
    public void PostedInventory_CannotBeCancelled()
    {
        var count = new InventoryCount(
            "INV-2026-0001",
            "Jahresinventur",
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        count.AddLine(Guid.NewGuid(), 10m);
        count.StartCounting();

        var line = Assert.Single(count.Lines);
        count.RecordCount(line.Id, 10m, null, "Max");
        count.MarkPosted("Max");

        Assert.Throws<InvalidOperationException>(() =>
            count.Cancel());
    }
}
