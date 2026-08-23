using WerkPilot.Domain.Offers;

namespace WerkPilot.UnitTests;

public sealed class OfferPositionTests
{
    [Fact]
    public void TotalNet_RoundsCommercially()
    {
        var position = new OfferPosition(1, "Test", 3m, 10.005m);
        Assert.Equal(30.02m, position.TotalNet);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidQuantity_Throws(decimal quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OfferPosition(1, "Test", quantity, 10m));
    }
}
