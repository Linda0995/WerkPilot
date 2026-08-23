using WerkPilot.Application.Calculation;
using WerkPilot.Application.Projects;
using WerkPilot.Application.TimeTracking;
using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Application.ProjectCosts;

public sealed class ProjectCostControllingService(
    IProjectRepository projectRepository,
    ICalculationRepository calculationRepository,
    IProjectActualCostRepository actualCostRepository,
    ProjectTimeControllingService timeControlling)
{
    public async Task<ProjectCostControllingDto> GetAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

        var actualCosts = await actualCostRepository.GetForProjectAsync(
            projectId, cancellationToken);

        var time = await timeControlling.GetAsync(projectId, cancellationToken);

        var actualMaterial = Sum(actualCosts, ProjectActualCostType.Material);
        var actualExternal = Sum(actualCosts, ProjectActualCostType.ExternalService);
        var actualOverhead = Sum(actualCosts, ProjectActualCostType.Overhead);
        var actualLabor = time.ActualLaborCostEstimate;

        if (!project.SourceOfferId.HasValue)
            return WithoutBudget(
                projectId, actualMaterial, actualLabor, actualExternal, actualOverhead);

        var calculation = await calculationRepository.GetByOfferIdAsync(
            project.SourceOfferId.Value, cancellationToken);

        if (calculation is null || calculation.TotalCost <= 0)
            return WithoutBudget(
                projectId, actualMaterial, actualLabor, actualExternal, actualOverhead);

        var plannedMaterial = calculation.MaterialCost;
        var plannedLabor = calculation.LaborCost;
        var plannedExternal = calculation.ExternalServiceCost;
        var plannedOverhead = calculation.OverheadCost;
        var plannedTotal = calculation.TotalCost;
        var actualTotal = actualMaterial + actualLabor + actualExternal + actualOverhead;

        var variance = decimal.Round(
            actualTotal - plannedTotal, 2, MidpointRounding.AwayFromZero);
        var remaining = decimal.Round(
            Math.Max(0m, plannedTotal - actualTotal), 2, MidpointRounding.AwayFromZero);
        var utilization = decimal.Round(
            actualTotal / plannedTotal * 100m, 1, MidpointRounding.AwayFromZero);

        var status = utilization switch
        {
            > 100m => ProjectCostControllingStatus.Exceeded,
            >= 85m => ProjectCostControllingStatus.Warning,
            _ => ProjectCostControllingStatus.OnTrack
        };

        return new ProjectCostControllingDto(
            projectId,
            plannedMaterial, actualMaterial,
            plannedLabor, actualLabor,
            plannedExternal, actualExternal,
            plannedOverhead, actualOverhead,
            plannedTotal, actualTotal,
            variance, remaining, utilization, status);
    }

    private static decimal Sum(
        IReadOnlyList<ProjectActualCost> costs,
        ProjectActualCostType type) =>
        decimal.Round(
            costs.Where(x => x.CostType == type).Sum(x => x.AmountNet),
            2,
            MidpointRounding.AwayFromZero);

    private static ProjectCostControllingDto WithoutBudget(
        Guid projectId,
        decimal actualMaterial,
        decimal actualLabor,
        decimal actualExternal,
        decimal actualOverhead)
    {
        var actualTotal = actualMaterial + actualLabor + actualExternal + actualOverhead;

        return new ProjectCostControllingDto(
            projectId,
            0m, actualMaterial,
            0m, actualLabor,
            0m, actualExternal,
            0m, actualOverhead,
            0m, actualTotal,
            actualTotal, 0m, 0m,
            ProjectCostControllingStatus.NoBudget);
    }
}
