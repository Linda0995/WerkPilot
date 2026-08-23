namespace WerkPilot.Domain.Purchasing;

public sealed class PurchaseListItem
{
    private PurchaseListItem() { }

    public PurchaseListItem(
        int positionNumber,
        Guid materialItemId,
        string articleNumber,
        string description,
        string unit,
        decimal requiredQuantity,
        decimal purchasePrice,
        string? supplier)
    {
        if (positionNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(positionNumber));
        if (materialItemId == Guid.Empty)
            throw new ArgumentException("Materialartikel erforderlich.", nameof(materialItemId));
        if (string.IsNullOrWhiteSpace(articleNumber))
            throw new ArgumentException("Artikelnummer erforderlich.", nameof(articleNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Einheit erforderlich.", nameof(unit));
        if (requiredQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(requiredQuantity));
        if (purchasePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(purchasePrice));

        Id = Guid.NewGuid();
        PositionNumber = positionNumber;
        MaterialItemId = materialItemId;
        ArticleNumber = articleNumber.Trim();
        Description = description.Trim();
        Unit = unit.Trim();
        RequiredQuantity = requiredQuantity;
        PurchasePrice = purchasePrice;
        Supplier = string.IsNullOrWhiteSpace(supplier) ? null : supplier.Trim();
    }

    public Guid Id { get; private init; }
    public int PositionNumber { get; private set; }
    public Guid MaterialItemId { get; private set; }
    public string ArticleNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal RequiredQuantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public string? Supplier { get; private set; }
    public bool IsOrdered { get; private set; }
    public DateTimeOffset? OrderedAtUtc { get; private set; }
    public string? OrderNote { get; private set; }

    public decimal EstimatedTotal => decimal.Round(
        RequiredQuantity * PurchasePrice,
        2,
        MidpointRounding.AwayFromZero);

    public void MarkOrdered(string? note, DateTimeOffset timestamp)
    {
        IsOrdered = true;
        OrderedAtUtc = timestamp;
        OrderNote = Clean(note);
    }

    public void MarkOpen()
    {
        IsOrdered = false;
        OrderedAtUtc = null;
        OrderNote = null;
    }

    public void UpdateOrderNote(string? note) =>
        OrderNote = Clean(note);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
