namespace WerkPilot.Domain.Billing;

public sealed class CustomerInvoiceLine
{
    private CustomerInvoiceLine() { }

    public CustomerInvoiceLine(
        string description,
        decimal quantity,
        string unit,
        decimal unitPriceNet,
        decimal vatRatePercent)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (quantity <= 0m)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPriceNet < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitPriceNet));
        if (vatRatePercent < 0m || vatRatePercent > 100m)
            throw new ArgumentOutOfRangeException(nameof(vatRatePercent));

        Id = Guid.NewGuid();
        Description = description.Trim();
        Quantity = quantity;
        Unit = string.IsNullOrWhiteSpace(unit) ? "Stk" : unit.Trim();
        UnitPriceNet = unitPriceNet;
        VatRatePercent = vatRatePercent;
    }

    public Guid Id { get; private init; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPriceNet { get; private set; }
    public decimal VatRatePercent { get; private set; }

    public decimal NetTotal => decimal.Round(
        Quantity * UnitPriceNet,
        2,
        MidpointRounding.AwayFromZero);

    public decimal VatAmount => decimal.Round(
        NetTotal * VatRatePercent / 100m,
        2,
        MidpointRounding.AwayFromZero);

    public decimal GrossTotal => NetTotal + VatAmount;
}
