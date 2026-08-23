using WerkPilot.Domain.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierOrderTests
{
    [Fact]
    public void PartialReceipt_ChangesStatus()
    {
        var order = new SupplierOrder(
            "BE-2026-0001",
            "Stahl GmbH",
            null,
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        order.AddLine(
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            "kg",
            10m,
            3m);

        order.MarkOrdered();
        var line = Assert.Single(order.Lines);
        order.Receive(line.Id, 4m);

        Assert.Equal(SupplierOrderStatus.PartiallyReceived, order.Status);
        Assert.Equal(6m, line.OpenQuantity);
    }

    [Fact]
    public void FullReceipt_ClosesOrder()
    {
        var order = new SupplierOrder(
            "BE-2026-0001",
            "Stahl GmbH",
            null,
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        order.AddLine(
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            "kg",
            10m,
            3m);

        order.MarkOrdered();
        var line = Assert.Single(order.Lines);
        order.Receive(line.Id, 10m);

        Assert.Equal(SupplierOrderStatus.Received, order.Status);
        Assert.Equal(0m, order.OpenQuantity);
        Assert.NotNull(order.ReceivedAtUtc);
    }

    [Fact]
    public void ReceivingMoreThanOpen_Throws()
    {
        var order = new SupplierOrder(
            "BE-2026-0001",
            "Stahl GmbH",
            null,
            new DateOnly(2026, 8, 2),
            null,
            "Max");

        order.AddLine(
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            "kg",
            10m,
            3m);

        order.MarkOrdered();
        var line = Assert.Single(order.Lines);

        Assert.Throws<InvalidOperationException>(() =>
            order.Receive(line.Id, 11m));
    }
}
