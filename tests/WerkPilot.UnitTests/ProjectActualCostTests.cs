using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.UnitTests;

public sealed class ProjectActualCostTests
{
    [Fact]
    public void Constructor_PreservesCostData()
    {
        var cost = new ProjectActualCost(
            Guid.NewGuid(),
            ProjectActualCostType.Material,
            "Stahl",
            1250m,
            new DateOnly(2026, 8, 2),
            "RE-100");

        Assert.Equal(1250m, cost.AmountNet);
        Assert.Equal("RE-100", cost.Reference);
    }

    [Fact]
    public void NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ProjectActualCost(
                Guid.NewGuid(),
                ProjectActualCostType.Overhead,
                "Kosten",
                -1m,
                DateOnly.FromDateTime(DateTime.Today),
                null));
    }
}
