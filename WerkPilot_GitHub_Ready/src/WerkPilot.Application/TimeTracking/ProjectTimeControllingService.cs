using WerkPilot.Application.Calculation;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Calculation;

namespace WerkPilot.Application.TimeTracking;

public sealed class ProjectTimeControllingService(
    IProjectRepository projectRepository,
    ICalculationRepository calculationRepository,
    ITimeEntryRepository timeEntryRepository)
{
    public async Task<ProjectTimeControllingDto> GetAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

        var entries = await timeEntryRepository.GetForProjectAsync(
            projectId,
            cancellationToken);

        var actualHours = decimal.Round(
            entries.Sum(x => x.DurationHours),
            2,
            MidpointRounding.AwayFromZero);

        if (!project.SourceOfferId.HasValue)
            return WithoutBudget(projectId, null, actualHours);

        var calculation = await calculationRepository.GetByOfferIdAsync(
            project.SourceOfferId.Value,
            cancellationToken);

        if (calculation is null)
            return WithoutBudget(projectId, project.SourceOfferId, actualHours);

        var laborItems = calculation.Items
            .Where(x => x.CostType == CostType.Labor)
            .ToArray();

        var plannedHours = decimal.Round(
            laborItems.Sum(x => x.Quantity),
            2,
            MidpointRounding.AwayFromZero);

        var plannedCost = decimal.Round(
            laborItems.Sum(x => x.TotalCost),
            2,
            MidpointRounding.AwayFromZero);

        if (plannedHours <= 0)
            return WithoutBudget(projectId, project.SourceOfferId, actualHours);

        var hourlyRate = decimal.Round(
            plannedCost / plannedHours,
            2,
            MidpointRounding.AwayFromZero);

        var actualCost = decimal.Round(
            actualHours * hourlyRate,
            2,
            MidpointRounding.AwayFromZero);

        var remainingHours = decimal.Round(
            Math.Max(0m, plannedHours - actualHours),
            2,
            MidpointRounding.AwayFromZero);

        var varianceHours = decimal.Round(
            actualHours - plannedHours,
            2,
            MidpointRounding.AwayFromZero);

        var utilization = decimal.Round(
            actualHours / plannedHours * 100m,
            1,
            MidpointRounding.AwayFromZero);

        var status = utilization switch
        {
            > 100m => ProjectTimeControllingStatus.Exceeded,
            >= 85m => ProjectTimeControllingStatus.Warning,
            _ => ProjectTimeControllingStatus.OnTrack
        };

        return new ProjectTimeControllingDto(
            projectId,
            project.SourceOfferId,
            plannedHours,
            plannedCost,
            hourlyRate,
            actualHours,
            actualCost,
            remainingHours,
            varianceHours,
            utilization,
            status);
    }

    private static ProjectTimeControllingDto WithoutBudget(
        Guid projectId,
        Guid? sourceOfferId,
        decimal actualHours) =>
        new(
            projectId,
            sourceOfferId,
            0m,
            0m,
            0m,
            actualHours,
            0m,
            0m,
            actualHours,
            0m,
            ProjectTimeControllingStatus.NoBudget);
}
