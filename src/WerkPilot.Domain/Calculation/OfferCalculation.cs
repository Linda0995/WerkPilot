using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Calculation;

public sealed class OfferCalculation : Entity
{
    private readonly List<CalculationItem> _items = [];
    private OfferCalculation() { }

    public OfferCalculation(Guid offerId)
    {
        if (offerId == Guid.Empty)
            throw new ArgumentException("Angebot erforderlich.", nameof(offerId));

        OfferId = offerId;
        ProfitTargetPercent = 20m;
    }

    public Guid OfferId { get; private set; }
    public decimal ProfitTargetPercent { get; private set; }
    public IReadOnlyCollection<CalculationItem> Items => _items.AsReadOnly();

    public decimal MaterialCost => Sum(CostType.Material);
    public decimal LaborCost => Sum(CostType.Labor);
    public decimal ExternalServiceCost => Sum(CostType.ExternalService);
    public decimal OverheadCost => Sum(CostType.Overhead);
    public decimal TotalCost => _items.Sum(x => x.TotalCost);
    public decimal TargetProfitAmount => decimal.Round(
        TotalCost * ProfitTargetPercent / 100m,
        2,
        MidpointRounding.AwayFromZero);
    public decimal RecommendedNetPrice => TotalCost + TargetProfitAmount;

    public CalculationItem AddItem(
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost,
        Guid? materialItemId = null)
    {
        var item = new CalculationItem(
            _items.Count + 1,
            costType,
            description,
            quantity,
            unitCost,
            materialItemId);

        _items.Add(item);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return item;
    }

    public void UpdateItem(
        Guid itemId,
        CostType costType,
        string description,
        decimal quantity,
        decimal unitCost)
    {
        var item = _items.SingleOrDefault(x => x.Id == itemId)
            ?? throw new InvalidOperationException("Kalkulationsposition wurde nicht gefunden.");

        item.Update(costType, description, quantity, unitCost);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.SingleOrDefault(x => x.Id == itemId)
            ?? throw new InvalidOperationException("Kalkulationsposition wurde nicht gefunden.");

        _items.Remove(item);

        for (var index = 0; index < _items.Count; index++)
            _items[index].SetPositionNumber(index + 1);

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetProfitTarget(decimal percent)
    {
        if (percent < 0 || percent > 500)
            throw new ArgumentOutOfRangeException(nameof(percent));

        ProfitTargetPercent = percent;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private decimal Sum(CostType type) =>
        _items.Where(x => x.CostType == type).Sum(x => x.TotalCost);
}
