using WerkPilot.Application.Materials;

namespace WerkPilot.Application.Inventory;

public sealed class InventoryValuationService(
    IInventoryRepository inventoryRepository,
    IMaterialRepository materialRepository,
    IInventoryValuationCsvExporter csvExporter)
{
    public async Task<InventoryValuationSummaryDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await inventoryRepository.GetAllAsync(cancellationToken);
        var result = new List<InventoryValuationItemDto>();

        foreach (var inventory in inventoryItems)
        {
            var material = await materialRepository.GetAsync(
                inventory.MaterialItemId,
                cancellationToken);

            if (material is null)
                continue;

            var stockValue = Round(inventory.QuantityOnHand * material.PurchasePrice);
            var reservedValue = Round(inventory.ReservedQuantity * material.PurchasePrice);
            var availableValue = Round(inventory.AvailableQuantity * material.PurchasePrice);
            var ageDays = Math.Max(
                0,
                (DateTimeOffset.UtcNow - material.PriceUpdatedAtUtc).Days);

            result.Add(new InventoryValuationItemDto(
                inventory.Id,
                inventory.MaterialItemId,
                material.ArticleNumber,
                material.Description,
                material.Unit,
                inventory.StorageLocation,
                inventory.QuantityOnHand,
                inventory.ReservedQuantity,
                inventory.AvailableQuantity,
                material.PurchasePrice,
                stockValue,
                reservedValue,
                availableValue,
                material.IsPriceOutdated(90),
                ageDays));
        }

        var ordered = result
            .OrderBy(x => x.StorageLocation)
            .ThenBy(x => x.ArticleNumber)
            .ToArray();

        return new InventoryValuationSummaryDto(
            Round(ordered.Sum(x => x.StockValue)),
            Round(ordered.Sum(x => x.ReservedValue)),
            Round(ordered.Sum(x => x.AvailableValue)),
            ordered.Length,
            ordered.Count(x => x.IsPriceOutdated),
            ordered);
    }

    public async Task<string> ExportCsvAsync(
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(await GetAsync(cancellationToken));

    private static decimal Round(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
