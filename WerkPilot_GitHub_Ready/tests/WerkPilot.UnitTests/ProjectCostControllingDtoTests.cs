using WerkPilot.Application.ProjectCosts;

namespace WerkPilot.UnitTests;

public sealed class ProjectCostControllingDtoTests
{
    [Fact]
    public void Dto_PreservesTotalsAndStatus()
    {
        var dto = new ProjectCostControllingDto(
            Guid.NewGuid(),
            1000m, 900m,
            2000m, 1800m,
            500m, 600m,
            300m, 250m,
            3800m, 3550m,
            -250m, 250m, 93.4m,
            ProjectCostControllingStatus.Warning);

        Assert.Equal(3800m, dto.PlannedTotalCost);
        Assert.Equal(3550m, dto.ActualTotalCost);
        Assert.Equal(ProjectCostControllingStatus.Warning, dto.Status);
    }
}
