using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Purchasing;

public sealed class PurchaseList : Entity
{
    private readonly List<PurchaseListItem> _items = [];
    private PurchaseList() { }

    public PurchaseList(string purchaseListNumber, Guid offerId, string title)
    {
        if (string.IsNullOrWhiteSpace(purchaseListNumber))
            throw new ArgumentException("Bestelllistennummer erforderlich.", nameof(purchaseListNumber));
        if (offerId == Guid.Empty)
            throw new ArgumentException("Angebot erforderlich.", nameof(offerId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Titel erforderlich.", nameof(title));

        PurchaseListNumber = purchaseListNumber.Trim();
        OfferId = offerId;
        Title = title.Trim();
        Status = PurchaseListStatus.Draft;
    }

    public string PurchaseListNumber { get; private set; } = string.Empty;
    public Guid OfferId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public PurchaseListStatus Status { get; private set; }
    public IReadOnlyCollection<PurchaseListItem> Items => _items.AsReadOnly();

    public int OrderedCount => _items.Count(x => x.IsOrdered);
    public int OpenCount => _items.Count - OrderedCount;
    public decimal EstimatedTotal => _items.Sum(x => x.EstimatedTotal);

    public PurchaseListItem AddItem(
        Guid materialItemId,
        string articleNumber,
        string description,
        string unit,
        decimal requiredQuantity,
        decimal purchasePrice,
        string? supplier)
    {
        EnsureEditable();

        var existing = _items.SingleOrDefault(x => x.MaterialItemId == materialItemId);
        if (existing is not null)
            throw new InvalidOperationException("Der Materialartikel ist bereits in der Bestellliste enthalten.");

        var item = new PurchaseListItem(
            _items.Count + 1,
            materialItemId,
            articleNumber,
            description,
            unit,
            requiredQuantity,
            purchasePrice,
            supplier);

        _items.Add(item);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return item;
    }

    public void ToggleOrdered(Guid itemId, string? note, DateTimeOffset timestamp)
    {
        EnsureEditable();

        var item = GetItem(itemId);

        if (item.IsOrdered)
            item.MarkOpen();
        else
            item.MarkOrdered(note, timestamp);

        Status = OrderedCount switch
        {
            0 => PurchaseListStatus.Draft,
            _ when OrderedCount == _items.Count && _items.Count > 0 => PurchaseListStatus.Completed,
            _ => PurchaseListStatus.InProgress
        };

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateNote(Guid itemId, string? note)
    {
        EnsureEditable();
        GetItem(itemId).UpdateOrderNote(note);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PurchaseListStatus.Completed)
            throw new InvalidOperationException("Eine abgeschlossene Bestellliste kann nicht storniert werden.");

        Status = PurchaseListStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private PurchaseListItem GetItem(Guid itemId) =>
        _items.SingleOrDefault(x => x.Id == itemId)
        ?? throw new InvalidOperationException("Bestellposition wurde nicht gefunden.");

    private void EnsureEditable()
    {
        if (Status == PurchaseListStatus.Cancelled)
            throw new InvalidOperationException("Eine stornierte Bestellliste kann nicht bearbeitet werden.");
    }
}
