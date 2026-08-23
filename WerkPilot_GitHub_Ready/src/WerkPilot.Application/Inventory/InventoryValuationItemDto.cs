namespace WerkPilot.Application.Inventory;

public sealed record InventoryValuationItemDto(
    Guid InventoryItemId,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    string? StorageLocation,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal PurchasePrice,
    decimal StockValue,
    decimal ReservedValue,
    decimal AvailableValue,
    bool IsPriceOutdated,
    int PriceAgeDays);
