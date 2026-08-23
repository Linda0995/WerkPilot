namespace WerkPilot.Application.Purchasing;

public interface IPurchaseListSource
{
    Task<IReadOnlyList<PurchaseListSourceItem>> GetItemsAsync(
        Guid offerId,
        CancellationToken cancellationToken = default);
}

public sealed record PurchaseListSourceItem(
    Guid MaterialItemId,
    string ArticleNumber,
    string Description,
    string Unit,
    decimal RequiredQuantity,
    decimal CurrentPurchasePrice,
    string? Supplier);
