using WerkPilot.Application.Release;
using WerkPilot.Domain.Billing;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class BasicWorkflowRulesTests
{
    [Fact]
    public void PaidInvoice_IsCompleted()
    {
        var stage = BasicWorkflowRules.ResolveStage(
            OfferStatus.Accepted,
            true,
            ProjectStatus.Completed,
            CustomerInvoiceStatus.Paid,
            0m,
            false);

        Assert.Equal("Abgeschlossen", stage);
    }

    [Fact]
    public void Dunning_HasPriorityOverOpenInvoiceStage()
    {
        var stage = BasicWorkflowRules.ResolveStage(
            OfferStatus.Accepted,
            true,
            ProjectStatus.Active,
            CustomerInvoiceStatus.Issued,
            1200m,
            true);

        Assert.Equal("Mahnung", stage);
    }

    [Fact]
    public void FullHappyPath_ReachesOneHundredPercent()
    {
        var completion = BasicWorkflowRules.CompletionPercent(
            true,
            true,
            true,
            true,
            true,
            true,
            true);

        Assert.Equal(100, completion);
    }

    [Fact]
    public void AcceptedOfferWithoutCalculation_IsVisible()
    {
        var stage = BasicWorkflowRules.ResolveStage(
            OfferStatus.Accepted,
            false,
            null,
            null,
            0m,
            false);

        Assert.Equal("Kalkulation fehlt", stage);
    }
}
