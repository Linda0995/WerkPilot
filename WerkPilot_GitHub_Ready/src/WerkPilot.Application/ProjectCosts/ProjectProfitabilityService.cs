using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;

namespace WerkPilot.Application.ProjectCosts;

public sealed class ProjectProfitabilityService(
    IProjectRepository projectRepository,
    OfferService offerService,
    ProjectCostControllingService costControlling)
{
    public async Task<ProjectProfitabilityDto> GetAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

        var costs = await costControlling.GetAsync(projectId, cancellationToken);

        if (!project.SourceOfferId.HasValue)
            return WithoutRevenue(projectId, null, costs);

        var offer = await offerService.GetAsync(
            project.SourceOfferId.Value,
            cancellationToken);

        var revenue = offer.NetTotal;

        if (revenue <= 0)
            return WithoutRevenue(projectId, project.SourceOfferId, costs);

        var plannedContribution = decimal.Round(
            revenue - costs.PlannedTotalCost,
            2,
            MidpointRounding.AwayFromZero);

        var actualContribution = decimal.Round(
            revenue - costs.ActualTotalCost,
            2,
            MidpointRounding.AwayFromZero);

        var plannedMargin = decimal.Round(
            plannedContribution / revenue * 100m,
            1,
            MidpointRounding.AwayFromZero);

        var actualMargin = decimal.Round(
            actualContribution / revenue * 100m,
            1,
            MidpointRounding.AwayFromZero);

        var resultVariance = decimal.Round(
            actualContribution - plannedContribution,
            2,
            MidpointRounding.AwayFromZero);

        var status = actualContribution switch
        {
            < 0m => ProjectProfitabilityStatus.Loss,
            _ when actualMargin < 10m => ProjectProfitabilityStatus.LowMargin,
            _ => ProjectProfitabilityStatus.Profitable
        };

        return new ProjectProfitabilityDto(
            projectId,
            project.SourceOfferId,
            revenue,
            costs.PlannedTotalCost,
            costs.ActualTotalCost,
            plannedContribution,
            actualContribution,
            plannedMargin,
            actualMargin,
            resultVariance,
            status);
    }

    private static ProjectProfitabilityDto WithoutRevenue(
        Guid projectId,
        Guid? sourceOfferId,
        ProjectCostControllingDto costs) =>
        new(
            projectId,
            sourceOfferId,
            0m,
            costs.PlannedTotalCost,
            costs.ActualTotalCost,
            -costs.PlannedTotalCost,
            -costs.ActualTotalCost,
            0m,
            0m,
            costs.PlannedTotalCost - costs.ActualTotalCost,
            ProjectProfitabilityStatus.NoRevenue);
}
