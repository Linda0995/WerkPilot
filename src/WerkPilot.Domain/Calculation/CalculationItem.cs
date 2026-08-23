namespace WerkPilot.Domain.Calculation;

public sealed class CalculationItem
{
    private CalculationItem() { }

    public CalculationItem(
        int positionNumber,
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost,
        Guid? materialItemId = null)
    {
        if (positionNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(positionNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitCost < 0)
            throw new ArgumentOutOfRangeException(nameof(unitCost));

        Id = Guid.NewGuid();
        PositionNumber = positionNumber;
        CostType = costType;
        Description = description.Trim();
        Quantity = quantity;
        UnitCost = unitCost;
        MaterialItemId = materialItemId;
    }

    public Guid Id { get; private init; }
    public int PositionNumber { get; private set; }
    public CostType CostType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public Guid? MaterialItemId { get; private set; }
    public decimal TotalCost => decimal.Round(Quantity * UnitCost, 2, MidpointRounding.AwayFromZero);

    public void Update(
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Beschreibung erforderlich.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitCost < 0)
            throw new ArgumentOutOfRangeException(nameof(unitCost));

        CostType = costType;
        Description = description.Trim();
        Quantity = quantity;
        UnitCost = unitCost;
    }

    internal void SetPositionNumber(int positionNumber) =>
        PositionNumber = positionNumber;
}
