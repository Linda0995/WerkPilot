namespace WerkPilot.Domain.Inventory;

public sealed class InventoryCountLine
{
    private InventoryCountLine() { }

    public InventoryCountLine(
        Guid inventoryItemId,
        decimal expectedQuantity)
    {
        if (inventoryItemId == Guid.Empty)
            throw new ArgumentException("Lagerartikel erforderlich.", nameof(inventoryItemId));
        if (expectedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedQuantity));

        Id = Guid.NewGuid();
        InventoryItemId = inventoryItemId;
        ExpectedQuantity = expectedQuantity;
    }

    public Guid Id { get; private init; }
    public Guid InventoryItemId { get; private set; }
    public decimal ExpectedQuantity { get; private set; }
    public decimal? CountedQuantity { get; private set; }
    public string? Note { get; private set; }
    public string? CountedBy { get; private set; }
    public DateTimeOffset? CountedAtUtc { get; private set; }
    public decimal DifferenceQuantity =>
        CountedQuantity.HasValue
            ? CountedQuantity.Value - ExpectedQuantity
            : 0m;

    public bool IsCounted => CountedQuantity.HasValue;

    public void RecordCount(
        decimal countedQuantity,
        string? note,
        string? countedBy)
    {
        if (countedQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(countedQuantity));

        CountedQuantity = countedQuantity;
        Note = Clean(note);
        CountedBy = Clean(countedBy);
        CountedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
