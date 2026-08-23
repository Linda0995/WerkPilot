using WerkPilot.Application.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierInvoiceLiquidityDtoTests
{
    [Fact]
    public void Summary_PreservesForecastBuckets()
    {
        var summary = new SupplierInvoiceLiquiditySummaryDto(
            10000m,
            1200m,
            2500m,
            4000m,
            8000m,
            150m,
            8,
            2,
            []);

        Assert.Equal(10000m, summary.TotalOpenAmount);
        Assert.Equal(1200m, summary.OverdueAmount);
        Assert.Equal(2500m, summary.DueWithin7Days);
        Assert.Equal(150m, summary.AvailableCashDiscount);
    }
}
