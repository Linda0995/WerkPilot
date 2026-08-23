using WerkPilot.Domain.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryMovementTests
{
    [Fact]
    public void Constructor_PreservesAuditData()
    {
        var projectId = Guid.NewGuid();
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            InventoryMovementType.Receipt,
            5m,
            "Wareneingang",
            DateTimeOffset.UtcNow,
            projectId,
            "LS-100",
            "Max");

        Assert.Equal(projectId, movement.ProjectId);
        Assert.Equal("LS-100", movement.Reference);
        Assert.Equal("Max", movement.CreatedBy);
    }
}
