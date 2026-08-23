using WerkPilot.Application.Billing;

namespace WerkPilot.UnitTests;

public sealed class ReceivablesSummaryDtoTests
{
    [Fact]
    public void Summary_PreservesBuckets()
    {
        var summary = new ReceivablesSummaryDto(
            10000m,
            1500m,
            2000m,
            4000m,
            8000m,
            10,
            2,
            []);

        Assert.Equal(10000m, summary.TotalOpenAmount);
        Assert.Equal(1500m, summary.OverdueAmount);
        Assert.Equal(2, summary.OverdueInvoiceCount);
    }
}
