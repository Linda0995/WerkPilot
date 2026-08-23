using WerkPilot.Application.Customers;
using WerkPilot.Domain.Customers;

namespace WerkPilot.UnitTests;

public sealed class Customer360DtoTests
{
    [Fact]
    public void Dto_PreservesOperationalSummary()
    {
        var customer = new CustomerDto(
            Guid.NewGuid(),
            "K-2026-0001",
            "Muster GmbH",
            CustomerType.Company,
            null,
            null,
            "8010",
            "Graz",
            "AT",
            null,
            null,
            null,
            "AT",
            "office@muster.at",
            null,
            null,
            TaxProfile.Domestic,
            null,
            false,
            false,
            DateTimeOffset.UtcNow,
            []);

        var dto = new Customer360Dto(
            customer,
            [],
            [],
            [],
            [],
            [],
            12500m,
            2,
            3);

        Assert.Equal(12500m, dto.OpenOfferVolumeNet);
        Assert.Equal(2, dto.ActiveProjectCount);
        Assert.Equal(3, dto.OpenFollowUpCount);
    }
}
