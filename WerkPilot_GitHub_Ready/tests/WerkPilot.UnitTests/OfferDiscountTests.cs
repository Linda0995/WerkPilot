using WerkPilot.Domain.Offers;

namespace WerkPilot.UnitTests;

public sealed class OfferDiscountTests
{
    [Fact]
    public void Discount_ReducesNetTaxAndGrossTotals()
    {
        var offer = CreateOffer();
        offer.AddPosition("Leistung", 1m, 100m);
        offer.SetDiscount(10m);

        Assert.Equal(100m, offer.PositionsNetTotal);
        Assert.Equal(10m, offer.DiscountAmount);
        Assert.Equal(90m, offer.NetTotal);
        Assert.Equal(18m, offer.TaxTotal);
        Assert.Equal(108m, offer.GrossTotal);
    }

    [Fact]
    public void OptionalPosition_IsExcludedFromOfferTotals()
    {
        var offer = CreateOffer();
        offer.AddPosition("Fix", 1m, 100m);
        offer.AddPosition("Alternative", 1m, 50m, true);

        Assert.Equal(100m, offer.PositionsNetTotal);
        Assert.Equal(150m, offer.Positions.Sum(x => x.TotalNet));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void SetDiscount_OutsideRange_Throws(decimal discount)
    {
        var offer = CreateOffer();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            offer.SetDiscount(discount));
    }

    [Fact]
    public void Copy_PreservesDiscountAndOptionalPositions()
    {
        var offer = CreateOffer();
        offer.AddPosition("Alternative", 1m, 50m, true);
        offer.SetDiscount(5m);

        var copy = offer.CreateCopy(
            "AN-2026-0002",
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)));

        Assert.Equal(5m, copy.DiscountPercent);
        Assert.True(copy.Positions.Single().IsOptional);
    }

    private static Offer CreateOffer() =>
        new(
            "AN-2026-0001",
            Guid.NewGuid(),
            "Test",
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            20m);
}
