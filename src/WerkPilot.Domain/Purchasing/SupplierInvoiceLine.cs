namespace WerkPilot.Domain.Purchasing;

public sealed class SupplierInvoiceLine
{
    private SupplierInvoiceLine() { }

    public SupplierInvoiceLine(
        Guid supplierOrderLineId,
        Guid materialItemId,
        string articleNumber,
        string description,
        decimal invoicedQuantity,
        decimal unitPriceNet)
    {
        if (supplierOrderLineId == Guid.Empty)
            throw new ArgumentException("Bestellposition erforderlich.", nameof(supplierOrderLineId));
        if (materialItemId == Guid.Empty)
            throw new ArgumentException("Materialartikel erforderlich.", nameof(materialItemId));
        if (string.IsNullOrWhiteSpace(articleNumber))
            throw new ArgumentException("Artikelnummer erforderlich.", nameof(articleNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (invoicedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(invoicedQuantity));
        if (unitPriceNet < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPriceNet));

        Id = Guid.NewGuid();
        SupplierOrderLineId = supplierOrderLineId;
        MaterialItemId = materialItemId;
        ArticleNumber = articleNumber.Trim();
        Description = description.Trim();
        InvoicedQuantity = invoicedQuantity;
        UnitPriceNet = unitPriceNet;
    }

    public Guid Id { get; private init; }
    public Guid SupplierOrderLineId { get; private set; }
    public Guid MaterialItemId { get; private set; }
    public string ArticleNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal InvoicedQuantity { get; private set; }
    public decimal UnitPriceNet { get; private set; }
    public decimal LineTotalNet => decimal.Round(
        InvoicedQuantity * UnitPriceNet,
        2,
        MidpointRounding.AwayFromZero);

    public void Update(decimal invoicedQuantity, decimal unitPriceNet)
    {
        if (invoicedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(invoicedQuantity));
        if (unitPriceNet < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPriceNet));

        InvoicedQuantity = invoicedQuantity;
        UnitPriceNet = unitPriceNet;
    }
}
