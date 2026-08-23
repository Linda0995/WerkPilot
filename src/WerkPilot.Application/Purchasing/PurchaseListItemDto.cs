namespace WerkPilot.Application.Purchasing;

public sealed record PurchaseListItemDto(
    Guid Id,
    int PositionNumber,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal RequiredQuantity,
    decimal PurchasePrice,
    decimal EstimatedTotal,
    string? Supplier,
    bool IsOrdered,
    DateTimeOffset? OrderedAtUtc,
    string? OrderNote);
