namespace WerkPilot.Application.TimeTracking;

public sealed record ProjectTimeControllingDto(
    Guid ProjectId,
    Guid? SourceOfferId,
    decimal PlannedLaborHours,
    decimal PlannedLaborCost,
    decimal AveragePlannedHourlyRate,
    decimal ActualHours,
    decimal ActualLaborCostEstimate,
    decimal RemainingHours,
    decimal VarianceHours,
    decimal UtilizationPercent,
    ProjectTimeControllingStatus Status);
