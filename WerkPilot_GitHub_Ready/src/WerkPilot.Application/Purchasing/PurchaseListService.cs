using WerkPilot.Application.Auditing;
using WerkPilot.Application.Calculation;
using WerkPilot.Application.Offers;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed class PurchaseListService(
    IPurchaseListRepository repository,
    IPurchaseListSource source,
    OfferService offerService,
    IAuditTrail auditTrail,
    IPurchaseListCsvExporter csvExporter)
{
    public async Task<IReadOnlyList<PurchaseListDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .Select(Map)
            .ToArray();

    public async Task<PurchaseListDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Map(await GetRequiredAsync(id, cancellationToken));

    public async Task<PurchaseListDto> CreateOrRefreshFromOfferAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByOfferIdAsync(offerId, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var offer = await offerService.GetAsync(offerId, cancellationToken);
        var sourceItems = await source.GetItemsAsync(offerId, cancellationToken);

        if (sourceItems.Count == 0)
            throw new InvalidOperationException(
                "Die Kalkulation enthält keine verknüpften Materialpositionen.");

        var number = await repository.GetNextNumberAsync(
            DateTime.Today.Year,
            cancellationToken);

        var purchaseList = new PurchaseList(
            number,
            offerId,
            $"Bestellliste zu {offer.OfferNumber} – {offer.Title}");

        foreach (var item in sourceItems)
        {
            purchaseList.AddItem(
                item.MaterialItemId,
                item.ArticleNumber,
                item.Description,
                item.Unit,
                item.RequiredQuantity,
                item.CurrentPurchasePrice,
                item.Supplier);
        }

        await repository.AddAsync(purchaseList, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "PurchaseList",
            purchaseList.Id,
            "Created",
            $"Bestellliste {purchaseList.PurchaseListNumber} wurde aus {offer.OfferNumber} erzeugt.",
            cancellationToken);

        return Map(purchaseList);
    }

    public async Task ToggleOrderedAsync(
        Guid purchaseListId,
        Guid itemId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var list = await GetRequiredAsync(purchaseListId, cancellationToken);
        list.ToggleOrdered(itemId, note, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "PurchaseList",
            list.Id,
            "OrderStateChanged",
            $"Bestellstatus der Position {itemId} wurde geändert.",
            cancellationToken);
    }

    public async Task UpdateNoteAsync(
        Guid purchaseListId,
        Guid itemId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var list = await GetRequiredAsync(purchaseListId, cancellationToken);
        list.UpdateNote(itemId, note);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ExportCsvAsync(
        Guid purchaseListId,
        CancellationToken cancellationToken = default)
    {
        var list = await GetRequiredAsync(purchaseListId, cancellationToken);
        return csvExporter.Export(Map(list));
    }

    private async Task<PurchaseList> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Bestellliste wurde nicht gefunden.");

    private static PurchaseListDto Map(PurchaseList x) => new(
        x.Id,
        x.PurchaseListNumber,
        x.OfferId,
        x.Title,
        x.Status,
        x.OrderedCount,
        x.OpenCount,
        x.EstimatedTotal,
        x.Items
            .OrderBy(item => item.Supplier)
            .ThenBy(item => item.PositionNumber)
            .Select(item => new PurchaseListItemDto(
                item.Id,
                item.PositionNumber,
                item.MaterialItemId,
                item.ArticleNumber,
                item.Description,
                item.Unit,
                item.RequiredQuantity,
                item.PurchasePrice,
                item.EstimatedTotal,
                item.Supplier,
                item.IsOrdered,
                item.OrderedAtUtc,
                item.OrderNote))
            .ToArray());
}
