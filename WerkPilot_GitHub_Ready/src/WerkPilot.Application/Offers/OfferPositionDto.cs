namespace WerkPilot.Application.Offers;

public sealed record OfferPositionDto(
    Guid Id,
    int PositionNumber,
    string Description,
    decimal Quantity,
    decimal UnitPriceNet,
    decimal TotalNet,
    bool IsOptional);
