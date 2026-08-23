using WerkPilot.Domain.Offers;

namespace WerkPilot.UnitTests;

public sealed class OfferTests
{
    [Fact]
    public void AddPosition_CalculatesTotals()
    {
        var offer = new Offer("AN-2026-0001", Guid.NewGuid(), "Geländer", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 20m);
        offer.AddPosition("Material", 2m, 100m);
        offer.AddPosition("Arbeit", 3m, 50m);

        Assert.Equal(350m, offer.NetTotal);
        Assert.Equal(70m, offer.TaxTotal);
        Assert.Equal(420m, offer.GrossTotal);
    }

    [Fact]
    public void MarkSent_WithoutPositions_Throws()
    {
        var offer = new Offer("AN-2026-0001", Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 20m);
        Assert.Throws<InvalidOperationException>(() => offer.MarkSent());
    }

    [Fact]
    public void StatusWorkflow_DraftSentAccepted_Works()
    {
        var offer = new Offer("AN-2026-0001", Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), 20m);
        offer.AddPosition("Position", 1m, 100m);
        offer.MarkSent();
        offer.Accept();

        Assert.Equal(OfferStatus.Accepted, offer.Status);
    }
}
