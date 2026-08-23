using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Purchasing;

public sealed class SupplierOrder : Entity
{
    private readonly List<SupplierOrderLine> _lines = [];
    private SupplierOrder() { }

    public SupplierOrder(
        string orderNumber,
        string supplierName,
        string? supplierReference,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Bestellnummer erforderlich.", nameof(orderNumber));
        if (string.IsNullOrWhiteSpace(supplierName))
            throw new ArgumentException("Lieferant erforderlich.", nameof(supplierName));

        OrderNumber = orderNumber.Trim();
        SupplierName = supplierName.Trim();
        SupplierReference = Clean(supplierReference);
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        CreatedBy = Clean(createdBy);
        Status = SupplierOrderStatus.Draft;
    }

    public string OrderNumber { get; private set; } = string.Empty;
    public string SupplierName { get; private set; } = string.Empty;
    public string? SupplierReference { get; private set; }
    public DateOnly OrderDate { get; private set; }
    public DateOnly? ExpectedDeliveryDate { get; private set; }
    public string? CreatedBy { get; private set; }
    public SupplierOrderStatus Status { get; private set; }
    public DateTimeOffset? OrderedAtUtc { get; private set; }
    public DateTimeOffset? ReceivedAtUtc { get; private set; }
    public IReadOnlyCollection<SupplierOrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalNet => _lines.Sum(x => x.LineTotalNet);
    public decimal OpenQuantity => _lines.Sum(x => x.OpenQuantity);

    public void AddLine(
        Guid materialItemId,
        string articleNumber,
        string description,
        string unit,
        decimal orderedQuantity,
        decimal unitPriceNet)
    {
        EnsureEditable();

        if (_lines.Any(x => x.MaterialItemId == materialItemId))
            throw new InvalidOperationException(
                "Der Materialartikel ist bereits in dieser Bestellung enthalten.");

        _lines.Add(new SupplierOrderLine(
            materialItemId,
            articleNumber,
            description,
            unit,
            orderedQuantity,
            unitPriceNet));

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkOrdered()
    {
        if (Status != SupplierOrderStatus.Draft)
            throw new InvalidOperationException("Nur Entwürfe können bestellt werden.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Eine Bestellung ohne Positionen kann nicht ausgelöst werden.");

        Status = SupplierOrderStatus.Ordered;
        OrderedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Receive(Guid lineId, decimal quantity)
    {
        if (Status is SupplierOrderStatus.Draft or SupplierOrderStatus.Cancelled or SupplierOrderStatus.Received)
            throw new InvalidOperationException("Für diese Bestellung kann kein Wareneingang gebucht werden.");

        var line = _lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Bestellposition wurde nicht gefunden.");

        line.Receive(quantity);

        Status = OpenQuantity == 0
            ? SupplierOrderStatus.Received
            : SupplierOrderStatus.PartiallyReceived;

        if (Status == SupplierOrderStatus.Received)
            ReceivedAtUtc = DateTimeOffset.UtcNow;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is SupplierOrderStatus.PartiallyReceived or SupplierOrderStatus.Received)
            throw new InvalidOperationException(
                "Eine teilweise oder vollständig gelieferte Bestellung kann nicht storniert werden.");

        Status = SupplierOrderStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status != SupplierOrderStatus.Draft)
            throw new InvalidOperationException("Nur Bestellentwürfe können bearbeitet werden.");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
