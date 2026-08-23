using WerkPilot.Application.Settings;

namespace WerkPilot.Application.Offers;

public sealed record OfferDocumentData(
    OfferDetailsDto Offer,
    CompanyProfileDto Company,
    string CustomerNumber,
    string CustomerName,
    string? ContactPerson,
    string? Street,
    string? PostalCode,
    string? City,
    string CountryCode,
    string? VatId);
