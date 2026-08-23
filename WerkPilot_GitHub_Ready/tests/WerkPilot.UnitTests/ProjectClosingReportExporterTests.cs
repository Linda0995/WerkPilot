using WerkPilot.Application.ProjectCosts;
using WerkPilot.Domain.Projects;
using WerkPilot.Infrastructure.ProjectCosts;

namespace WerkPilot.UnitTests;

public sealed class ProjectClosingReportExporterTests
{
    [Fact]
    public void CsvExport_ContainsProjectAndContributionMargin()
    {
        var costs = new ProjectCostControllingDto(
            Guid.NewGuid(), 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m,
            100m, 90m, -10m, 10m, 90m,
            ProjectCostControllingStatus.Warning);

        var result = new ProjectProfitabilityDto(
            Guid.NewGuid(), null, 150m, 100m, 90m,
            50m, 60m, 33.3m, 40m, 10m,
            ProjectProfitabilityStatus.Profitable);

        var report = new ProjectClosingReportDto(
            Guid.NewGuid(), "PR-1", "Projekt", "Kunde", null,
            ProjectStatus.Completed, new DateOnly(2026, 8, 1), null,
            100, 0, DateTimeOffset.UtcNow, costs, result,
            10m, 9m, -1m, 90m, "OK", true);

        var csv = new ProjectClosingReportExporter().ExportCsv(report);

        Assert.Contains("PR-1", csv);
        Assert.Contains("Aktueller Deckungsbeitrag", csv);
        Assert.Contains("Abschlussbereit", csv);
    }
}
