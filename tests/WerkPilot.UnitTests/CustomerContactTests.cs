using WerkPilot.Domain.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerContactTests
{
    [Fact]
    public void AddContact_AsPrimary_MakesOnlyNewContactPrimary()
    {
        var customer = new Customer("K-1", "Test GmbH", CustomerType.Company);
        var first = customer.AddContact("Technik", "technik@test.at", null, true);
        var second = customer.AddContact("Einkauf", "einkauf@test.at", null, true);

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
    }

    [Fact]
    public void SetPrimaryContact_UpdatesMainContactFields()
    {
        var customer = new Customer("K-1", "Test GmbH", CustomerType.Company);
        var contact = customer.AddContact("Einkauf", "einkauf@test.at", "+43 1", false);

        customer.SetPrimaryContact(contact.Id);

        Assert.Equal("Einkauf", customer.ContactPerson);
        Assert.Equal("einkauf@test.at", customer.Email);
        Assert.Equal("+43 1", customer.Phone);
        Assert.True(contact.IsPrimary);
    }

    [Fact]
    public void RemovePrimaryContact_PromotesRemainingContact()
    {
        var customer = new Customer("K-1", "Test GmbH", CustomerType.Company);
        var primary = customer.AddContact("Technik", null, null, true);
        var remaining = customer.AddContact("Einkauf", null, null, false);

        customer.RemoveContact(primary.Id);

        Assert.True(remaining.IsPrimary);
    }
}
