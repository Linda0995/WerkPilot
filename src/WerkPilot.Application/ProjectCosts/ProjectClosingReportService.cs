using WerkPilot.Application.Customers;
using WerkPilot.Application.Projects;
using WerkPilot.Application.TimeTracking;

namespace WerkPilot.Application.ProjectCosts;

public sealed class ProjectClosingReportService(
    IProjectRepository projectRepository,
    CustomerService customerService,
    ProjectCostControllingService costControlling,
    ProjectProfitabilityService profitability,
    ProjectTimeControllingService timeControlling,
    IProjectClosingReportExporter exporter)
{
    public async Task<ProjectClosingReportDto> CreateAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        var customerName = customers
            .SingleOrDefault(x => x.Id == project.CustomerId)?
            .DisplayName ?? "Unbekannter Kunde";

        var costs = await costControlling.GetAsync(projectId, cancellationToken);
        var result = await profitability.GetAsync(projectId, cancellationToken);
        var time = await timeControlling.GetAsync(projectId, cancellationToken);

        var canBeClosed = project.OpenTaskCount == 0;
        var assessment = BuildAssessment(
            canBeClosed,
            result.Status,
            costs.Status,
            time.Status);

        return new ProjectClosingReportDto(
            project.Id,
            project.ProjectNumber,
            project.Title,
            customerName,
            project.ProjectManager,
            project.Status,
            project.PlannedStart,
            project.PlannedEnd,
            project.ProgressPercent,
            project.OpenTaskCount,
            DateTimeOffset.UtcNow,
            costs,
            result,
            time.PlannedLaborHours,
            time.ActualHours,
            time.VarianceHours,
            time.UtilizationPercent,
            assessment,
            canBeClosed);
    }

    public async Task<string> ExportCsvAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        exporter.ExportCsv(await CreateAsync(projectId, cancellationToken));

    public async Task<string> ExportMarkdownAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        exporter.ExportMarkdown(await CreateAsync(projectId, cancellationToken));

    private static string BuildAssessment(
        bool canBeClosed,
        ProjectProfitabilityStatus profitability,
        ProjectCostControllingStatus costs,
        ProjectTimeControllingStatus time)
    {
        if (!canBeClosed)
            return "Projekt besitzt noch offene Aufgaben und ist fachlich nicht abschlussbereit.";

        if (profitability == ProjectProfitabilityStatus.Loss)
            return "Projekt ist organisatorisch abschlussbereit, weist jedoch aktuell einen Verlust aus.";

        if (costs == ProjectCostControllingStatus.Exceeded ||
            time == ProjectTimeControllingStatus.Exceeded)
            return "Projekt ist abschlussbereit, jedoch wurde mindestens ein Budget überschritten.";

        if (profitability == ProjectProfitabilityStatus.LowMargin ||
            costs == ProjectCostControllingStatus.Warning ||
            time == ProjectTimeControllingStatus.Warning)
            return "Projekt ist abschlussbereit. Die Nachkalkulation enthält mindestens einen Warnhinweis.";

        return "Projekt ist fachlich und wirtschaftlich abschlussbereit.";
    }
}
