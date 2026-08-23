namespace WerkPilot.Application.ProjectCosts;

public sealed record ProjectProfitabilityDto(
    Guid ProjectId,
    Guid? SourceOfferId,
    decimal RevenueNet,
    decimal PlannedCost,
    decimal ActualCost,
    decimal PlannedContributionMargin,
    decimal ActualContributionMargin,
    decimal PlannedMarginPercent,
    decimal ActualMarginPercent,
    decimal ResultVariance,
    ProjectProfitabilityStatus Status);
