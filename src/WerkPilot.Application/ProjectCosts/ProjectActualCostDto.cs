using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Application.ProjectCosts;

public sealed record ProjectActualCostDto(
    Guid Id,
    Guid ProjectId,
    ProjectActualCostType CostType,
    string Description,
    decimal AmountNet,
    DateOnly CostDate,
    string? Reference);
