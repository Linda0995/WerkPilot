namespace WerkPilot.Application.Inventory;

public sealed record InventoryCountLineDto(
    Guid Id,
    Guid InventoryItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    string? StorageLocation,
    decimal ExpectedQuantity,
    decimal? CountedQuantity,
    decimal DifferenceQuantity,
    decimal PurchasePrice,
    decimal DifferenceValue,
    bool IsPriceOutdated,
    string? Note,
    string? CountedBy,
    DateTimeOffset? CountedAtUtc,
    bool IsCounted);
