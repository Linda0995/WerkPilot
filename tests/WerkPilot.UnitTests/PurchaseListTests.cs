using WerkPilot.Domain.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class PurchaseListTests
{
    [Fact]
    public void AddItem_CalculatesEstimatedTotal()
    {
        var list = CreateList();

        var item = list.AddItem(
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            "kg",
            10m,
            3.50m,
            "Stahl GmbH");

        Assert.Equal(35m, item.EstimatedTotal);
        Assert.Equal(35m, list.EstimatedTotal);
        Assert.Equal(1, list.OpenCount);
    }

    [Fact]
    public void ToggleOrdered_UpdatesStatus()
    {
        var list = CreateList();
        var first = list.AddItem(Guid.NewGuid(), "A", "Artikel A", "Stk", 1m, 10m, null);
        var second = list.AddItem(Guid.NewGuid(), "B", "Artikel B", "Stk", 1m, 20m, null);

        list.ToggleOrdered(first.Id, "Telefonisch", DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseListStatus.InProgress, list.Status);
        Assert.Equal(1, list.OrderedCount);

        list.ToggleOrdered(second.Id, null, DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseListStatus.Completed, list.Status);
        Assert.Equal(0, list.OpenCount);
    }

    [Fact]
    public void ToggleOrdered_Again_ReopensPosition()
    {
        var list = CreateList();
        var item = list.AddItem(Guid.NewGuid(), "A", "Artikel A", "Stk", 1m, 10m, null);

        list.ToggleOrdered(item.Id, "Bestellt", DateTimeOffset.UtcNow);
        list.ToggleOrdered(item.Id, null, DateTimeOffset.UtcNow);

        Assert.False(item.IsOrdered);
        Assert.Equal(PurchaseListStatus.Draft, list.Status);
    }

    [Fact]
    public void DuplicateMaterial_IsRejected()
    {
        var list = CreateList();
        var materialId = Guid.NewGuid();

        list.AddItem(materialId, "A", "Artikel A", "Stk", 1m, 10m, null);

        Assert.Throws<InvalidOperationException>(() =>
            list.AddItem(materialId, "A", "Artikel A", "Stk", 2m, 10m, null));
    }

    private static PurchaseList CreateList() =>
        new("BL-2026-0001", Guid.NewGuid(), "Bestellliste");
}
