using WerkPilot.Domain.Calculation;

namespace WerkPilot.UnitTests;

public sealed class CalculationMaterialLinkTests
{
    [Fact]
    public void AddItem_PreservesMaterialReference()
    {
        var materialId = Guid.NewGuid();
        var calculation = new OfferCalculation(Guid.NewGuid());

        var item = calculation.AddItem(
            CostType.Material,
            "MAT-001 – Stahl",
            2m,
            3m,
            materialId);

        Assert.Equal(materialId, item.MaterialItemId);
    }
}
