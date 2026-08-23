using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Settings;

public sealed class CompanyProfile : Entity
{
    private CompanyProfile() { }

    public CompanyProfile(string companyName)
    {
        UpdateCompany(companyName, null, null, null, "AT", null, null, null, null);
        OfferIntroText = "Vielen Dank für Ihre Anfrage. Gerne bieten wir Ihnen folgende Leistungen an:";
        OfferClosingText = "Wir freuen uns auf Ihre Rückmeldung und stehen für Fragen gerne zur Verfügung.";
        CurrencyCode = "EUR";
        OfferEmailSubjectTemplate = "Angebot {OfferNumber} – {OfferTitle}";
        OfferEmailBodyTemplate =
            "Sehr geehrte Damen und Herren,\n\n"
            + "anbei erhalten Sie unser Angebot {OfferNumber}.\n\n"
            + "Mit freundlichen Grüßen\n{CompanyName}";
    }

    public string CompanyName { get; private set; } = string.Empty;
    public string? Street { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string CountryCode { get; private set; } = "AT";
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? VatId { get; private set; }
    public string? Website { get; private set; }
    public string OfferIntroText { get; private set; } = string.Empty;
    public string OfferClosingText { get; private set; } = string.Empty;
    public string CurrencyCode { get; private set; } = "EUR";
    public string OfferEmailSubjectTemplate { get; private set; } = string.Empty;
    public string OfferEmailBodyTemplate { get; private set; } = string.Empty;

    public void UpdateCompany(
        string companyName,
        string? street,
        string? postalCode,
        string? city,
        string countryCode,
        string? email,
        string? phone,
        string? vatId,
        string? website)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Der Firmenname ist erforderlich.", nameof(companyName));

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2)
            throw new ArgumentException("Der Ländercode muss aus zwei Buchstaben bestehen.", nameof(countryCode));

        CompanyName = companyName.Trim();
        Street = Clean(street);
        PostalCode = Clean(postalCode);
        City = Clean(city);
        CountryCode = countryCode.Trim().ToUpperInvariant();
        Email = Clean(email);
        Phone = Clean(phone);
        VatId = Clean(vatId)?.ToUpperInvariant();
        Website = Clean(website);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateOfferTexts(string introText, string closingText, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(introText))
            throw new ArgumentException("Der Einleitungstext ist erforderlich.", nameof(introText));
        if (string.IsNullOrWhiteSpace(closingText))
            throw new ArgumentException("Der Abschlusstext ist erforderlich.", nameof(closingText));
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
            throw new ArgumentException("Der Währungscode muss aus drei Buchstaben bestehen.", nameof(currencyCode));

        OfferIntroText = introText.Trim();
        OfferClosingText = closingText.Trim();
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateOfferEmailTemplate(string subjectTemplate, string bodyTemplate)
    {
        if (string.IsNullOrWhiteSpace(subjectTemplate))
            throw new ArgumentException("Die E-Mail-Betreffvorlage ist erforderlich.", nameof(subjectTemplate));

        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new ArgumentException("Die E-Mail-Nachrichtenvorlage ist erforderlich.", nameof(bodyTemplate));

        OfferEmailSubjectTemplate = subjectTemplate.Trim();
        OfferEmailBodyTemplate = bodyTemplate.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
