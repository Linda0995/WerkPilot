using WerkPilot.Domain.Calculation;

namespace WerkPilot.Application.Calculation;

public sealed record CalculationItemDto(
    Guid Id,
    int PositionNumber,
    CostType CostType,
    string Description,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    Guid? MaterialItemId);
