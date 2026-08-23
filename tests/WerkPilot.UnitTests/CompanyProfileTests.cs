using WerkPilot.Domain.Settings;

namespace WerkPilot.UnitTests;

public sealed class CompanyProfileTests
{
    [Fact]
    public void Constructor_SetsUsefulOfferDefaults()
    {
        var profile = new CompanyProfile("Muster GmbH");

        Assert.Equal("Muster GmbH", profile.CompanyName);
        Assert.Equal("EUR", profile.CurrencyCode);
        Assert.False(string.IsNullOrWhiteSpace(profile.OfferIntroText));
        Assert.False(string.IsNullOrWhiteSpace(profile.OfferClosingText));
    }

    [Fact]
    public void UpdateOfferTexts_NormalizesCurrency()
    {
        var profile = new CompanyProfile("Muster GmbH");

        profile.UpdateOfferTexts("Einleitung", "Abschluss", "eur");

        Assert.Equal("EUR", profile.CurrencyCode);
    }

    [Fact]
    public void UpdateCompany_WithInvalidCountryCode_Throws()
    {
        var profile = new CompanyProfile("Muster GmbH");

        Assert.Throws<ArgumentException>(() =>
            profile.UpdateCompany("Muster GmbH", null, null, null, "AUT", null, null, null, null));
    }
}
