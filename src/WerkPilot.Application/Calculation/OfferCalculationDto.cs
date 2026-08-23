namespace WerkPilot.Application.Calculation;

public sealed record OfferCalculationDto(
    Guid Id,
    Guid OfferId,
    decimal ProfitTargetPercent,
    decimal MaterialCost,
    decimal LaborCost,
    decimal ExternalServiceCost,
    decimal OverheadCost,
    decimal TotalCost,
    decimal TargetProfitAmount,
    decimal RecommendedNetPrice,
    IReadOnlyList<CalculationItemDto> Items);
