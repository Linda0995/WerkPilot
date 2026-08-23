using System.Net.Mail;
using WerkPilot.Application.Common;

namespace WerkPilot.Application.Customers;

public static class CustomerContactValidator
{
    public static ValidationResult Validate(AddCustomerContactRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Label))
            result.Add(nameof(request.Label), "Die Bezeichnung des Ansprechpartners ist erforderlich.");
        else if (request.Label.Trim().Length > 100)
            result.Add(nameof(request.Label), "Die Bezeichnung darf höchstens 100 Zeichen enthalten.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            result.Add(nameof(request.Email), "Die E-Mail-Adresse des Ansprechpartners ist ungültig.");

        if (request.Phone?.Trim().Length > 50)
            result.Add(nameof(request.Phone), "Die Telefonnummer darf höchstens 50 Zeichen enthalten.");

        return result;
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
}
