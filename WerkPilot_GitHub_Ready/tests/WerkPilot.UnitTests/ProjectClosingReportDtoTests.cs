using WerkPilot.Application.ProjectCosts;
using WerkPilot.Domain.Projects;

namespace WerkPilot.UnitTests;

public sealed class ProjectClosingReportDtoTests
{
    [Fact]
    public void Report_PreservesClosingAssessment()
    {
        var costs = new ProjectCostControllingDto(
            Guid.NewGuid(),
            100m, 90m, 200m, 190m, 0m, 0m, 20m, 20m,
            320m, 300m, -20m, 20m, 93.8m,
            ProjectCostControllingStatus.Warning);

        var profitability = new ProjectProfitabilityDto(
            Guid.NewGuid(), Guid.NewGuid(), 500m, 320m, 300m,
            180m, 200m, 36m, 40m, 20m,
            ProjectProfitabilityStatus.Profitable);

        var report = new ProjectClosingReportDto(
            Guid.NewGuid(), "PR-2026-0001", "Test", "Muster GmbH",
            "Max", ProjectStatus.Active, new DateOnly(2026, 8, 1), null,
            100, 0, DateTimeOffset.UtcNow, costs, profitability,
            10m, 9m, -1m, 90m,
            "Projekt ist abschlussbereit.", true);

        Assert.True(report.CanBeClosed);
        Assert.Equal("Projekt ist abschlussbereit.", report.ClosingAssessment);
    }
}
