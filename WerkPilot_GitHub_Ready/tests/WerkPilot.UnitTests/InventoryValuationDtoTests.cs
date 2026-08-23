using WerkPilot.Application.Inventory;

namespace WerkPilot.UnitTests;

public sealed class InventoryValuationDtoTests
{
    [Fact]
    public void Summary_PreservesStockReservedAndAvailableValues()
    {
        var summary = new InventoryValuationSummaryDto(
            1000m,
            250m,
            750m,
            4,
            1,
            []);

        Assert.Equal(1000m, summary.TotalStockValue);
        Assert.Equal(250m, summary.TotalReservedValue);
        Assert.Equal(750m, summary.TotalAvailableValue);
        Assert.Equal(1, summary.OutdatedPriceCount);
    }
}
