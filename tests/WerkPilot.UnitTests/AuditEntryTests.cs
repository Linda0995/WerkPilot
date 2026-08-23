using WerkPilot.Infrastructure.Persistence;

namespace WerkPilot.UnitTests;

public sealed class AuditEntryTests
{
    [Fact]
    public void Constructor_SetsAuditData()
    {
        var entityId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var entry = new AuditEntry(
            "Customer",
            entityId,
            "Updated",
            "Kunde wurde geändert.",
            timestamp);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("Customer", entry.EntityType);
        Assert.Equal(entityId, entry.EntityId);
        Assert.Equal("Updated", entry.Action);
        Assert.Equal(timestamp, entry.OccurredAtUtc);
    }
}
