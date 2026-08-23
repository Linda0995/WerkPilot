namespace WerkPilot.Domain.Customers;

public sealed class Address
{
    private Address() { }

    public Address(string? street, string? postalCode, string? city, string countryCode)
    {
        Street = Clean(street);
        PostalCode = Clean(postalCode);
        City = Clean(city);
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "AT" : countryCode.Trim().ToUpperInvariant();
    }

    public string? Street { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string CountryCode { get; private set; } = "AT";

    public string DisplayText => string.Join(", ", new[] { Street, $"{PostalCode} {City}".Trim(), CountryCode }
        .Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
