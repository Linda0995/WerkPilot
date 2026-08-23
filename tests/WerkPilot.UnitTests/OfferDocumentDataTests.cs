using WerkPilot.Application.Offers;
using WerkPilot.Application.Settings;
using WerkPilot.Domain.Offers;

namespace WerkPilot.UnitTests;

public sealed class OfferDocumentDataTests
{
    [Fact]
    public void DocumentData_PreservesOfferAndCustomerData()
    {
        var offer = new OfferDetailsDto(
            Guid.NewGuid(),
            "AN-2026-0001",
            Guid.NewGuid(),
            "Geländer",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 9, 1),
            OfferStatus.Draft,
            20m,
            0m,
            100m,
            0m,
            100m,
            20m,
            120m,
            []);

        var company = new CompanyProfileDto(
            Guid.NewGuid(),
            "WerkPilot Musterbetrieb",
            "Firmenstraße 1",
            "8010",
            "Graz",
            "AT",
            "office@werkpilot.at",
            "+43 123",
            "ATU12345678",
            "https://werkpilot.at",
            "Einleitung",
            "Abschluss",
            "EUR",
            "Angebot {OfferNumber}",
            "Guten Tag {CustomerName}");

        var document = new OfferDocumentData(
            offer,
            company,
            "K-2026-0001",
            "Muster GmbH",
            "Max Muster",
            "Werkstraße 1",
            "8010",
            "Graz",
            "AT",
            "ATU12345678");

        Assert.Equal("AN-2026-0001", document.Offer.OfferNumber);
        Assert.Equal("Muster GmbH", document.CustomerName);
        Assert.Equal(120m, document.Offer.GrossTotal);
    }
}
