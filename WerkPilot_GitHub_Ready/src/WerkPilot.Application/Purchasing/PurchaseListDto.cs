using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed record PurchaseListDto(
    Guid Id,
    string PurchaseListNumber,
    Guid OfferId,
    string Title,
    PurchaseListStatus Status,
    int OrderedCount,
    int OpenCount,
    decimal EstimatedTotal,
    IReadOnlyList<PurchaseListItemDto> Items);
