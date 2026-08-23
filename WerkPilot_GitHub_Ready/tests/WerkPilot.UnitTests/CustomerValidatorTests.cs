using WerkPilot.Application.Customers;
using WerkPilot.Domain.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerValidatorTests
{
    [Fact]
    public void Validate_WithValidData_IsValid()
    {
        var request = CreateRequest(email: "office@example.at", countryCode: "AT");
        Assert.True(CustomerValidator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var request = CreateRequest(displayName: " ");
        var result = CustomerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.DisplayName));
    }

    [Fact]
    public void Validate_WithInvalidEmail_ReturnsError()
    {
        var request = CreateRequest(email: "keine-email");
        var result = CustomerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.Email));
    }

    [Fact]
    public void Validate_WithInvalidCountryCode_ReturnsError()
    {
        var request = CreateRequest(countryCode: "AUT");
        var result = CustomerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.BillingCountryCode));
    }

    private static UpdateCustomerRequest CreateRequest(
        string displayName = "Muster GmbH",
        string? email = null,
        string countryCode = "AT") =>
        new(
            Guid.NewGuid(),
            displayName,
            CustomerType.Company,
            "Max Muster",
            "Werkstraße 1",
            "8010",
            "Graz",
            countryCode,
            null,
            null,
            null,
            countryCode,
            true,
            email,
            "+43 123",
            "ATU12345678",
            TaxProfile.Domestic,
            null);
}
