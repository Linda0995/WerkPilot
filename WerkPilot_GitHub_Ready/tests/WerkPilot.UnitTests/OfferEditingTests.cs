using WerkPilot.Domain.Offers;

namespace WerkPilot.UnitTests;

public sealed class OfferEditingTests
{
    [Fact]
    public void UpdatePosition_ChangesTotals()
    {
        var offer = CreateDraft();
        var position = offer.AddPosition("Material", 1m, 100m);

        offer.UpdatePosition(position.Id, "Material neu", 2m, 125m, false);

        Assert.Equal(250m, offer.NetTotal);
        Assert.Equal("Material neu", offer.Positions.Single().Description);
    }

    [Fact]
    public void RemovePosition_RenumbersRemainingPositions()
    {
        var offer = CreateDraft();
        var first = offer.AddPosition("A", 1m, 10m);
        offer.AddPosition("B", 1m, 20m);

        offer.RemovePosition(first.Id);

        var remaining = Assert.Single(offer.Positions);
        Assert.Equal(1, remaining.PositionNumber);
    }

    [Fact]
    public void CreateCopy_CreatesDraftWithCopiedPositions()
    {
        var offer = CreateDraft();
        offer.AddPosition("A", 2m, 50m);

        var copy = offer.CreateCopy(
            "AN-2026-0002",
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)));

        Assert.Equal(OfferStatus.Draft, copy.Status);
        Assert.Equal(100m, copy.NetTotal);
        Assert.Contains("Kopie", copy.Title);
    }

    [Fact]
    public void MarkExpired_ExpiresOverdueSentOffer()
    {
        var offer = new Offer(
            "AN-2026-0001",
            Guid.NewGuid(),
            "Test",
            new DateOnly(2026, 7, 31),
            20m);

        offer.AddPosition("A", 1m, 10m);
        offer.MarkSent();
        offer.MarkExpired(new DateOnly(2026, 8, 2));

        Assert.Equal(OfferStatus.Expired, offer.Status);
    }

    private static Offer CreateDraft() =>
        new(
            "AN-2026-0001",
            Guid.NewGuid(),
            "Test",
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            20m);
}
