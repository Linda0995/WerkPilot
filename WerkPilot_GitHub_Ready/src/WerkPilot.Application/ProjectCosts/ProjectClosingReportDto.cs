using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.ProjectCosts;

public sealed record ProjectClosingReportDto(
    Guid ProjectId,
    string ProjectNumber,
    string ProjectTitle,
    string CustomerName,
    string? ProjectManager,
    ProjectStatus ProjectStatus,
    DateOnly PlannedStart,
    DateOnly? PlannedEnd,
    int ProgressPercent,
    int OpenTaskCount,
    DateTimeOffset GeneratedAtUtc,
    ProjectCostControllingDto CostControlling,
    ProjectProfitabilityDto Profitability,
    decimal PlannedLaborHours,
    decimal ActualLaborHours,
    decimal LaborVarianceHours,
    decimal LaborUtilizationPercent,
    string ClosingAssessment,
    bool CanBeClosed);
