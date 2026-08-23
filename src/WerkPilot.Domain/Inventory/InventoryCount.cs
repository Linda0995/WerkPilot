using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Inventory;

public sealed class InventoryCount : Entity
{
    private readonly List<InventoryCountLine> _lines = [];
    private InventoryCount() { }

    public InventoryCount(
        string countNumber,
        string title,
        DateOnly countDate,
        string? storageLocation,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(countNumber))
            throw new ArgumentException("Inventurnummer erforderlich.", nameof(countNumber));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Inventurbezeichnung erforderlich.", nameof(title));

        CountNumber = countNumber.Trim();
        Title = title.Trim();
        CountDate = countDate;
        StorageLocation = Clean(storageLocation);
        CreatedBy = Clean(createdBy);
        Status = InventoryCountStatus.Draft;
    }

    public string CountNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateOnly CountDate { get; private set; }
    public string? StorageLocation { get; private set; }
    public string? CreatedBy { get; private set; }
    public InventoryCountStatus Status { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }
    public string? PostedBy { get; private set; }
    public IReadOnlyCollection<InventoryCountLine> Lines => _lines.AsReadOnly();

    public int CountedLineCount => _lines.Count(x => x.IsCounted);
    public int OpenLineCount => _lines.Count - CountedLineCount;
    public decimal AbsoluteDifferenceQuantity =>
        _lines.Where(x => x.IsCounted).Sum(x => Math.Abs(x.DifferenceQuantity));

    public void AddLine(Guid inventoryItemId, decimal expectedQuantity)
    {
        EnsureEditable();

        if (_lines.Any(x => x.InventoryItemId == inventoryItemId))
            return;

        _lines.Add(new InventoryCountLine(inventoryItemId, expectedQuantity));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void StartCounting()
    {
        EnsureEditable();

        if (_lines.Count == 0)
            throw new InvalidOperationException("Eine Inventur ohne Positionen kann nicht gestartet werden.");

        Status = InventoryCountStatus.Counting;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RecordCount(
        Guid lineId,
        decimal countedQuantity,
        string? note,
        string? countedBy)
    {
        if (Status is not InventoryCountStatus.Counting and not InventoryCountStatus.ReadyForPosting)
            throw new InvalidOperationException("Die Inventur ist nicht zur Zählung freigegeben.");

        var line = _lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Inventurposition wurde nicht gefunden.");

        line.RecordCount(countedQuantity, note, countedBy);

        Status = OpenLineCount == 0
            ? InventoryCountStatus.ReadyForPosting
            : InventoryCountStatus.Counting;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkPosted(string? postedBy)
    {
        if (Status != InventoryCountStatus.ReadyForPosting)
            throw new InvalidOperationException(
                "Nur vollständig gezählte Inventuren können gebucht werden.");

        Status = InventoryCountStatus.Posted;
        PostedBy = Clean(postedBy);
        PostedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InventoryCountStatus.Posted)
            throw new InvalidOperationException("Eine gebuchte Inventur kann nicht storniert werden.");

        Status = InventoryCountStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status is InventoryCountStatus.Posted or InventoryCountStatus.Cancelled)
            throw new InvalidOperationException(
                "Gebuchte oder stornierte Inventuren können nicht bearbeitet werden.");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
