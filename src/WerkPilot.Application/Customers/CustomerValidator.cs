using System.Net.Mail;
using System.Text.RegularExpressions;
using WerkPilot.Application.Common;

namespace WerkPilot.Application.Customers;

public static partial class CustomerValidator
{
    public static ValidationResult Validate(UpdateCustomerRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            result.Add(nameof(request.DisplayName), "Der Kundenname ist erforderlich.");
        else if (request.DisplayName.Trim().Length > 200)
            result.Add(nameof(request.DisplayName), "Der Kundenname darf höchstens 200 Zeichen enthalten.");

        ValidateOptionalLength(result, nameof(request.ContactPerson), request.ContactPerson, 150, "Ansprechpartner");
        ValidateOptionalLength(result, nameof(request.BillingStreet), request.BillingStreet, 200, "Straße");
        ValidateOptionalLength(result, nameof(request.BillingPostalCode), request.BillingPostalCode, 20, "Postleitzahl");
        ValidateOptionalLength(result, nameof(request.BillingCity), request.BillingCity, 100, "Ort");
        ValidateOptionalLength(result, nameof(request.Phone), request.Phone, 50, "Telefonnummer");
        ValidateOptionalLength(result, nameof(request.Notes), request.Notes, 4000, "Notizen");

        if (string.IsNullOrWhiteSpace(request.BillingCountryCode) || !CountryCodeRegex().IsMatch(request.BillingCountryCode.Trim()))
            result.Add(nameof(request.BillingCountryCode), "Der Ländercode der Rechnungsadresse muss aus zwei Buchstaben bestehen.");

        if (!request.DeliveryAddressEqualsBillingAddress &&
            (string.IsNullOrWhiteSpace(request.DeliveryCountryCode) || !CountryCodeRegex().IsMatch(request.DeliveryCountryCode.Trim())))
            result.Add(nameof(request.DeliveryCountryCode), "Der Ländercode der Lieferadresse muss aus zwei Buchstaben bestehen.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            result.Add(nameof(request.Email), "Die E-Mail-Adresse ist ungültig.");

        if (!string.IsNullOrWhiteSpace(request.VatId) && request.VatId.Trim().Length > 30)
            result.Add(nameof(request.VatId), "Die UID-/ATU-Nummer darf höchstens 30 Zeichen enthalten.");

        return result;
    }

    public static ValidationResult ValidateNewCustomer(string displayName)
    {
        var request = new UpdateCustomerRequest(
            Guid.Empty,
            displayName,
            Domain.Customers.CustomerType.Company,
            null,
            null,
            null,
            null,
            "AT",
            null,
            null,
            null,
            "AT",
            true,
            null,
            null,
            null,
            Domain.Customers.TaxProfile.Inland,
            null);

        return Validate(request);
    }

    private static void ValidateOptionalLength(
        ValidationResult result,
        string propertyName,
        string? value,
        int maximumLength,
        string label)
    {
        if (value?.Trim().Length > maximumLength)
            result.Add(propertyName, $"{label} darf höchstens {maximumLength} Zeichen enthalten.");
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value.Trim());
            return string.Equals(address.Address, value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    [GeneratedRegex("^[A-Za-z]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex CountryCodeRegex();
}
