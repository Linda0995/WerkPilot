namespace WerkPilot.Application.Inventory;

public sealed record ReorderSuggestionDto(
    Guid InventoryItemId,
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    string? Supplier,
    string? SupplierArticleNumber,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal OpenDemandQuantity,
    decimal MinimumStock,
    decimal SuggestedOrderQuantity,
    decimal PurchasePrice,
    decimal EstimatedOrderValue,
    bool IsPriceOutdated);
