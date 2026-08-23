namespace WerkPilot.Application.Settings;

public sealed record UpdateCompanyProfileRequest(
    string CompanyName,
    string? Street,
    string? PostalCode,
    string? City,
    string CountryCode,
    string? Email,
    string? Phone,
    string? VatId,
    string? Website,
    string OfferIntroText,
    string OfferClosingText,
    string CurrencyCode,
    string OfferEmailSubjectTemplate,
    string OfferEmailBodyTemplate);
