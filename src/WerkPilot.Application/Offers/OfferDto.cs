using WerkPilot.Domain.Offers;

namespace WerkPilot.Application.Offers;

public sealed record OfferDto(
    Guid Id,
    string OfferNumber,
    Guid CustomerId,
    string Title,
    DateOnly OfferDate,
    DateOnly ValidUntil,
    OfferStatus Status,
    decimal NetTotal,
    decimal TaxTotal,
    decimal GrossTotal,
    int PositionCount);
