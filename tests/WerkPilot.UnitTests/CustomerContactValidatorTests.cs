using WerkPilot.Application.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerContactValidatorTests
{
    [Fact]
    public void Validate_WithoutLabel_IsInvalid()
    {
        var request = new AddCustomerContactRequest(Guid.NewGuid(), " ", null, null, false);
        Assert.False(CustomerContactValidator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_WithInvalidEmail_IsInvalid()
    {
        var request = new AddCustomerContactRequest(Guid.NewGuid(), "Einkauf", "falsch", null, false);
        Assert.False(CustomerContactValidator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_WithValidContact_IsValid()
    {
        var request = new AddCustomerContactRequest(
            Guid.NewGuid(), "Einkauf", "einkauf@example.at", "+43 1", true);

        Assert.True(CustomerContactValidator.Validate(request).IsValid);
    }
}
