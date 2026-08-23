using WerkPilot.Domain.Settings;

namespace WerkPilot.UnitTests;

public sealed class OfferEmailTemplateTests
{
    [Fact]
    public void Constructor_CreatesDefaultEmailTemplate()
    {
        var profile = new CompanyProfile("Muster GmbH");

        Assert.Contains("{OfferNumber}", profile.OfferEmailSubjectTemplate);
        Assert.Contains("{CompanyName}", profile.OfferEmailBodyTemplate);
    }

    [Fact]
    public void UpdateOfferEmailTemplate_StoresValues()
    {
        var profile = new CompanyProfile("Muster GmbH");

        profile.UpdateOfferEmailTemplate(
            "Angebot {OfferNumber}",
            "Guten Tag {CustomerName}");

        Assert.Equal("Angebot {OfferNumber}", profile.OfferEmailSubjectTemplate);
        Assert.Equal("Guten Tag {CustomerName}", profile.OfferEmailBodyTemplate);
    }

    [Fact]
    public void UpdateOfferEmailTemplate_RejectsEmptySubject()
    {
        var profile = new CompanyProfile("Muster GmbH");

        Assert.Throws<ArgumentException>(() =>
            profile.UpdateOfferEmailTemplate("", "Nachricht"));
    }
}
