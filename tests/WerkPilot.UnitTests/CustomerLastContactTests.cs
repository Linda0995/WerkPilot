using WerkPilot.Domain.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerLastContactTests
{
    [Fact]
    public void RegisterContact_KeepsNewestTimestamp()
    {
        var customer = new Customer("K-2026-0001", "Muster GmbH", CustomerType.Company);
        var newer = DateTimeOffset.UtcNow;
        var older = newer.AddDays(-10);

        customer.RegisterContact(newer);
        customer.RegisterContact(older);

        Assert.Equal(newer, customer.LastContactAtUtc);
    }
}
