namespace WerkPilot.Application.Calculation;

public sealed record PurchaseListItemDto(
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal RequiredQuantity,
    decimal CurrentPurchasePrice,
    decimal EstimatedTotal,
    string? Supplier,
    bool IsPriceOutdated);
