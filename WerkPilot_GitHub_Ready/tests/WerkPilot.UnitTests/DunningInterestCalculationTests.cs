namespace WerkPilot.UnitTests;

public sealed class DunningInterestCalculationTests
{
    [Fact]
    public void InterestFormula_UsesOpenAmountRateAndDays()
    {
        var openAmount = 1000m;
        var annualRate = 9.2m;
        var days = 30;

        var interest = decimal.Round(
            openAmount * annualRate / 100m * days / 365m,
            2,
            MidpointRounding.AwayFromZero);

        Assert.Equal(7.56m, interest);
    }
}
