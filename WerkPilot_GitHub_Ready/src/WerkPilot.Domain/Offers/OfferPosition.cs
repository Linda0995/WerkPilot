namespace WerkPilot.Domain.Offers;

public sealed class OfferPosition
{
    private OfferPosition() { }

    public OfferPosition(int positionNumber, string description, decimal quantity, decimal unitPriceNet, bool isOptional = false)
    {
        if (positionNumber <= 0) throw new ArgumentOutOfRangeException(nameof(positionNumber));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPriceNet < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceNet));

        Id = Guid.NewGuid();
        PositionNumber = positionNumber;
        Description = description.Trim();
        Quantity = quantity;
        UnitPriceNet = unitPriceNet;
        IsOptional = isOptional;
    }

    public Guid Id { get; private init; }
    public int PositionNumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPriceNet { get; private set; }
    public bool IsOptional { get; private set; }
    public decimal TotalNet => decimal.Round(Quantity * UnitPriceNet, 2, MidpointRounding.AwayFromZero);

    public void Update(string description, decimal quantity, decimal unitPriceNet, bool isOptional)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPriceNet < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPriceNet));

        Description = description.Trim();
        Quantity = quantity;
        UnitPriceNet = unitPriceNet;
        IsOptional = isOptional;
    }

    internal void SetPositionNumber(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        PositionNumber = value;
    }
}
