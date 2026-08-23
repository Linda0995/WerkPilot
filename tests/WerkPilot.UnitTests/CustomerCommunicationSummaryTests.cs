using WerkPilot.Application.Customers;

namespace WerkPilot.UnitTests;

public sealed class CustomerCommunicationSummaryTests
{
    [Fact]
    public void Summary_PreservesCommunicationCounters()
    {
        var summary = new CustomerCommunicationSummaryDto(
            Guid.NewGuid(),
            "KD-0001",
            "Muster GmbH",
            "office@example.com",
            DateTimeOffset.UtcNow,
            7,
            6,
            1,
            2,
            3,
            1,
            1,
            []);

        Assert.Equal(7, summary.TotalCount);
        Assert.Equal(3, summary.InvoiceCount);
        Assert.Equal(1, summary.FailedCount);
    }
}
