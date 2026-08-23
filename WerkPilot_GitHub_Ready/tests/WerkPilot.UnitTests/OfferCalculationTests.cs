using WerkPilot.Domain.Calculation;

namespace WerkPilot.UnitTests;

public sealed class OfferCalculationTests
{
    [Fact]
    public void CostsAndRecommendedPrice_AreCalculated()
    {
        var calculation = new OfferCalculation(Guid.NewGuid());
        calculation.AddItem(CostType.Material, "Stahl", 2m, 100m);
        calculation.AddItem(CostType.Labor, "Montage", 4m, 50m);
        calculation.AddItem(CostType.Overhead, "Gemeinkosten", 1m, 40m);
        calculation.SetProfitTarget(25m);

        Assert.Equal(200m, calculation.MaterialCost);
        Assert.Equal(200m, calculation.LaborCost);
        Assert.Equal(40m, calculation.OverheadCost);
        Assert.Equal(440m, calculation.TotalCost);
        Assert.Equal(110m, calculation.TargetProfitAmount);
        Assert.Equal(550m, calculation.RecommendedNetPrice);
    }

    [Fact]
    public void RemoveItem_RenumbersRemainingItems()
    {
        var calculation = new OfferCalculation(Guid.NewGuid());
        var first = calculation.AddItem(CostType.Material, "A", 1m, 10m);
        calculation.AddItem(CostType.Labor, "B", 1m, 20m);

        calculation.RemoveItem(first.Id);

        var remaining = Assert.Single(calculation.Items);
        Assert.Equal(1, remaining.PositionNumber);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(500.01)]
    public void SetProfitTarget_OutsideRange_Throws(decimal percent)
    {
        var calculation = new OfferCalculation(Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            calculation.SetProfitTarget(percent));
    }

    [Fact]
    public void UpdateItem_ChangesCostSummary()
    {
        var calculation = new OfferCalculation(Guid.NewGuid());
        var item = calculation.AddItem(CostType.Material, "Material", 1m, 100m);

        calculation.UpdateItem(
            item.Id,
            CostType.ExternalService,
            "Laserzuschnitt",
            2m,
            75m);

        Assert.Equal(0m, calculation.MaterialCost);
        Assert.Equal(150m, calculation.ExternalServiceCost);
    }
}
