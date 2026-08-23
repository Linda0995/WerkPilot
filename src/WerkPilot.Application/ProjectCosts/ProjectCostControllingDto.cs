namespace WerkPilot.Application.ProjectCosts;

public sealed record ProjectCostControllingDto(
    Guid ProjectId,
    decimal PlannedMaterialCost,
    decimal ActualMaterialCost,
    decimal PlannedLaborCost,
    decimal ActualLaborCost,
    decimal PlannedExternalServiceCost,
    decimal ActualExternalServiceCost,
    decimal PlannedOverheadCost,
    decimal ActualOverheadCost,
    decimal PlannedTotalCost,
    decimal ActualTotalCost,
    decimal VarianceAmount,
    decimal RemainingBudget,
    decimal UtilizationPercent,
    ProjectCostControllingStatus Status);
