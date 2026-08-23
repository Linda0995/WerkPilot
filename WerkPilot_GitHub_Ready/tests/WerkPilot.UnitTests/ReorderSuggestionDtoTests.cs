using WerkPilot.Application.Inventory;

namespace WerkPilot.UnitTests;

public sealed class ReorderSuggestionDtoTests
{
    [Fact]
    public void Dto_PreservesDemandAndOrderValue()
    {
        var dto = new ReorderSuggestionDto(
            Guid.NewGuid(), Guid.NewGuid(), "MAT-001", "Stahlblech", "kg",
            "Stahl GmbH", "S355-01", 20m, 5m, 15m, 10m, 8m,
            3m, 2.50m, 7.50m, false);

        Assert.Equal(15m, dto.AvailableQuantity);
        Assert.Equal(10m, dto.OpenDemandQuantity);
        Assert.Equal(3m, dto.SuggestedOrderQuantity);
        Assert.Equal(7.50m, dto.EstimatedOrderValue);
    }
}
