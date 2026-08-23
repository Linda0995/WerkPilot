using WerkPilot.Application.Purchasing;

namespace WerkPilot.Application.Calculation;

public sealed class CalculationPurchaseListSource(WerkPilot.Application.Calculation.PurchaseListService service)
    : IPurchaseListSource
{
    public async Task<IReadOnlyList<PurchaseListSourceItem>> GetItemsAsync(
        Guid offerId,
        CancellationToken cancellationToken = default) =>
        (await service.CreateAsync(offerId, cancellationToken))
            .Select(x => new PurchaseListSourceItem(
                x.MaterialItemId,
                x.ArticleNumber,
                x.Description,
                x.Unit,
                x.RequiredQuantity,
                x.CurrentPurchasePrice,
                x.Supplier))
            .ToArray();
}
