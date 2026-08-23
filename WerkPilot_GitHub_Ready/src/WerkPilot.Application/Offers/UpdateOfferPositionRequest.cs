namespace WerkPilot.Application.Offers;

public sealed record UpdateOfferPositionRequest(
    Guid OfferId,
    Guid PositionId,
    string Description,
    decimal Quantity,
    decimal UnitPriceNet,
    bool IsOptional);
