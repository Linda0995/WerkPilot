namespace WerkPilot.Domain.Purchasing;

public sealed class SupplierOrderLine
{
    private SupplierOrderLine() { }

    public SupplierOrderLine(
        Guid materialItemId,
        string articleNumber,
        string description,
        string unit,
        decimal orderedQuantity,
        decimal unitPriceNet)
    {
        if (materialItemId == Guid.Empty)
            throw new ArgumentException("Materialartikel erforderlich.", nameof(materialItemId));
        if (string.IsNullOrWhiteSpace(articleNumber))
            throw new ArgumentException("Artikelnummer erforderlich.", nameof(articleNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (orderedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderedQuantity));
        if (unitPriceNet < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPriceNet));

        Id = Guid.NewGuid();
        MaterialItemId = materialItemId;
        ArticleNumber = articleNumber.Trim();
        Description = description.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "Stk" : unit.Trim();
        OrderedQuantity = orderedQuantity;
        UnitPriceNet = unitPriceNet;
    }

    public Guid Id { get; private init; }
    public Guid MaterialItemId { get; private set; }
    public string ArticleNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal OrderedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitPriceNet { get; private set; }
    public decimal OpenQuantity => OrderedQuantity - ReceivedQuantity;
    public decimal LineTotalNet => decimal.Round(
        OrderedQuantity * UnitPriceNet,
        2,
        MidpointRounding.AwayFromZero);

    public void Receive(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > OpenQuantity)
            throw new InvalidOperationException(
                "Die Wareneingangsmenge überschreitet die offene Bestellmenge.");

        ReceivedQuantity += quantity;
    }
}
