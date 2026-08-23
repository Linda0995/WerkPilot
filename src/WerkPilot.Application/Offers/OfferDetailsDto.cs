using WerkPilot.Domain.Offers;

namespace WerkPilot.Application.Offers;

public sealed record OfferDetailsDto(
    Guid Id,
    string OfferNumber,
    Guid CustomerId,
    string Title,
    DateOnly OfferDate,
    DateOnly ValidUntil,
    OfferStatus Status,
    decimal TaxRate,
    decimal DiscountPercent,
    decimal PositionsNetTotal,
    decimal DiscountAmount,
    decimal NetTotal,
    decimal TaxTotal,
    decimal GrossTotal,
    IReadOnlyList<OfferPositionDto> Positions);
