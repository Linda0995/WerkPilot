namespace WerkPilot.Application.Inventory;

public sealed record InventoryValuationSummaryDto(
    decimal TotalStockValue,
    decimal TotalReservedValue,
    decimal TotalAvailableValue,
    int InventoryItemCount,
    int OutdatedPriceCount,
    IReadOnlyList<InventoryValuationItemDto> Items);
