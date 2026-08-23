using WerkPilot.Application.Materials;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Application.Inventory;

public sealed class ReorderSuggestionService(
    IInventoryRepository inventoryRepository,
    IMaterialRepository materialRepository,
    IPurchaseListRepository purchaseListRepository,
    IReorderSuggestionCsvExporter csvExporter)
{
    public async Task<IReadOnlyList<ReorderSuggestionDto>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await inventoryRepository.GetAllAsync(cancellationToken);
        var purchaseLists = await purchaseListRepository.GetAllAsync(cancellationToken);

        var openDemand = purchaseLists
            .Where(x => x.Status is not WerkPilot.Domain.Purchasing.PurchaseListStatus.Cancelled)
            .SelectMany(x => x.Items)
            .Where(x => !x.IsOrdered)
            .GroupBy(x => x.MaterialItemId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(item => item.RequiredQuantity));

        var suggestions = new List<ReorderSuggestionDto>();

        foreach (var inventory in inventoryItems)
        {
            var material = await materialRepository.GetAsync(
                inventory.MaterialItemId,
                cancellationToken);

            if (material is null)
                continue;

            var demand = openDemand.GetValueOrDefault(inventory.MaterialItemId);
            var targetQuantity = demand + inventory.MinimumStock;
            var suggestedQuantity = decimal.Round(
                Math.Max(0m, targetQuantity - inventory.AvailableQuantity),
                3,
                MidpointRounding.AwayFromZero);

            if (suggestedQuantity <= 0)
                continue;

            suggestions.Add(new ReorderSuggestionDto(
                inventory.Id,
                inventory.MaterialItemId,
                material.ArticleNumber,
                material.Description,
                material.Unit,
                material.Supplier,
                material.SupplierArticleNumber,
                inventory.QuantityOnHand,
                inventory.ReservedQuantity,
                inventory.AvailableQuantity,
                demand,
                inventory.MinimumStock,
                suggestedQuantity,
                material.PurchasePrice,
                decimal.Round(
                    suggestedQuantity * material.PurchasePrice,
                    2,
                    MidpointRounding.AwayFromZero),
                material.IsPriceOutdated(90)));
        }

        return suggestions
            .OrderBy(x => x.Supplier)
            .ThenBy(x => x.ArticleNumber)
            .ToArray();
    }

    public async Task<string> ExportCsvAsync(
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(await GetAsync(cancellationToken));
}
