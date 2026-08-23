namespace WerkPilot.Application.Inventory;

public sealed record InventoryItemDto(
    Guid Id,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    string? StorageLocation,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal MinimumStock,
    bool IsBelowMinimum);
