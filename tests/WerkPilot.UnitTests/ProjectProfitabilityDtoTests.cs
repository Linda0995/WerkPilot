using WerkPilot.Application.ProjectCosts;

namespace WerkPilot.UnitTests;

public sealed class ProjectProfitabilityDtoTests
{
    [Fact]
    public void Dto_PreservesActualMargin()
    {
        var dto = new ProjectProfitabilityDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10000m,
            7000m,
            8000m,
            3000m,
            2000m,
            30m,
            20m,
            -1000m,
            ProjectProfitabilityStatus.Profitable);

        Assert.Equal(2000m, dto.ActualContributionMargin);
        Assert.Equal(20m, dto.ActualMarginPercent);
        Assert.Equal(-1000m, dto.ResultVariance);
    }

    [Theory]
    [InlineData(ProjectProfitabilityStatus.NoRevenue)]
    [InlineData(ProjectProfitabilityStatus.Profitable)]
    [InlineData(ProjectProfitabilityStatus.LowMargin)]
    [InlineData(ProjectProfitabilityStatus.Loss)]
    public void Statuses_AreDefined(ProjectProfitabilityStatus status) =>
        Assert.True(Enum.IsDefined(status));
}
