using WerkPilot.Domain.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryItemTests
{
    [Fact]
    public void Receipt_IncreasesStock()
    {
        var item = new InventoryItem(Guid.NewGuid(), "A-01", 2m);

        item.ApplyMovement(InventoryMovementType.Receipt, 10m);

        Assert.Equal(10m, item.QuantityOnHand);
        Assert.Equal(10m, item.AvailableQuantity);
    }

    [Fact]
    public void Reservation_ReducesAvailableQuantity()
    {
        var item = new InventoryItem(Guid.NewGuid(), "A-01", 2m);
        item.ApplyMovement(InventoryMovementType.Receipt, 10m);

        item.ApplyMovement(InventoryMovementType.Reservation, 4m);

        Assert.Equal(4m, item.ReservedQuantity);
        Assert.Equal(6m, item.AvailableQuantity);
    }

    [Fact]
    public void IssueBeyondStock_Throws()
    {
        var item = new InventoryItem(Guid.NewGuid(), "A-01", 0m);
        item.ApplyMovement(InventoryMovementType.Receipt, 2m);

        Assert.Throws<InvalidOperationException>(() =>
            item.ApplyMovement(InventoryMovementType.Issue, 3m));
    }

    [Fact]
    public void MinimumStock_UsesAvailableQuantity()
    {
        var item = new InventoryItem(Guid.NewGuid(), "A-01", 5m);
        item.ApplyMovement(InventoryMovementType.Receipt, 10m);
        item.ApplyMovement(InventoryMovementType.Reservation, 6m);

        Assert.True(item.IsBelowMinimum);
    }
}
