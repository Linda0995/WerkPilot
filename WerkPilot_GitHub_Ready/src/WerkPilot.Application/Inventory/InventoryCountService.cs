using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Materials;
using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public sealed class InventoryCountService(
    IInventoryCountRepository countRepository,
    IInventoryRepository inventoryRepository,
    IMaterialRepository materialRepository,
    InventoryService inventoryService,
    SessionContext session,
    IInventoryCountCsvExporter csvExporter,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<InventoryCountDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var counts = await countRepository.GetAllAsync(cancellationToken);
        var result = new List<InventoryCountDto>();

        foreach (var count in counts)
            result.Add(await MapAsync(count, cancellationToken));

        return result
            .OrderByDescending(x => x.CountDate)
            .ThenByDescending(x => x.CountNumber)
            .ToArray();
    }

    public async Task<InventoryCountDto> CreateAsync(
        string title,
        DateOnly countDate,
        string? storageLocation,
        CancellationToken cancellationToken = default)
    {
        var number = await countRepository.GetNextNumberAsync(
            countDate.Year,
            cancellationToken);

        var count = new InventoryCount(
            number,
            title,
            countDate,
            storageLocation,
            session.DisplayName);

        var inventoryItems = await inventoryRepository.GetAllAsync(cancellationToken);

        foreach (var item in inventoryItems
                     .Where(x =>
                         string.IsNullOrWhiteSpace(storageLocation) ||
                         string.Equals(
                             x.StorageLocation,
                             storageLocation.Trim(),
                             StringComparison.OrdinalIgnoreCase)))
        {
            count.AddLine(item.Id, item.QuantityOnHand);
        }

        await countRepository.AddAsync(count, cancellationToken);
        await countRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "InventoryCount",
            count.Id,
            "Created",
            $"Inventur {count.CountNumber} wurde mit {count.Lines.Count} Positionen angelegt.",
            cancellationToken);

        return await MapAsync(count, cancellationToken);
    }

    public async Task StartAsync(
        Guid countId,
        CancellationToken cancellationToken = default)
    {
        var count = await GetRequiredAsync(countId, cancellationToken);
        count.StartCounting();
        await countRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordCountAsync(
        Guid countId,
        Guid lineId,
        decimal countedQuantity,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var count = await GetRequiredAsync(countId, cancellationToken);
        count.RecordCount(
            lineId,
            countedQuantity,
            note,
            session.DisplayName);

        await countRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task PostAsync(
        Guid countId,
        CancellationToken cancellationToken = default)
    {
        var count = await GetRequiredAsync(countId, cancellationToken);

        if (count.Status != InventoryCountStatus.ReadyForPosting)
            throw new InvalidOperationException(
                "Die Inventur ist noch nicht vollständig gezählt.");

        foreach (var line in count.Lines.Where(x => x.IsCounted))
        {
            var difference = line.DifferenceQuantity;

            if (difference == 0)
                continue;

            await inventoryService.BookMovementAsync(
                line.InventoryItemId,
                difference > 0
                    ? InventoryMovementType.AdjustmentIncrease
                    : InventoryMovementType.AdjustmentDecrease,
                Math.Abs(difference),
                $"Inventurkorrektur {count.CountNumber}",
                null,
                count.CountNumber,
                cancellationToken);
        }

        count.MarkPosted(session.DisplayName);
        await countRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "InventoryCount",
            count.Id,
            "Posted",
            $"Inventur {count.CountNumber} wurde gebucht.",
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid countId,
        CancellationToken cancellationToken = default)
    {
        var count = await GetRequiredAsync(countId, cancellationToken);
        count.Cancel();
        await countRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ExportCsvAsync(
        Guid countId,
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(
            await MapAsync(
                await GetRequiredAsync(countId, cancellationToken),
                cancellationToken));

    private async Task<InventoryCount> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await countRepository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Inventur wurde nicht gefunden.");

    private async Task<InventoryCountDto> MapAsync(
        InventoryCount count,
        CancellationToken cancellationToken)
    {
        var lines = new List<InventoryCountLineDto>();

        foreach (var line in count.Lines.OrderBy(x => x.InventoryItemId))
        {
            var inventory = await inventoryRepository.GetAsync(
                line.InventoryItemId,
                cancellationToken);

            if (inventory is null)
                continue;

            var material = await materialRepository.GetAsync(
                inventory.MaterialItemId,
                cancellationToken);

            if (material is null)
                continue;

            lines.Add(new InventoryCountLineDto(
                line.Id,
                line.InventoryItemId,
                material.ArticleNumber,
                material.Description,
                material.Unit,
                inventory.StorageLocation,
                line.ExpectedQuantity,
                line.CountedQuantity,
                line.DifferenceQuantity,
                material.PurchasePrice,
                decimal.Round(
                    line.DifferenceQuantity * material.PurchasePrice,
                    2,
                    MidpointRounding.AwayFromZero),
                material.IsPriceOutdated(90),
                line.Note,
                line.CountedBy,
                line.CountedAtUtc,
                line.IsCounted));
        }

        return new InventoryCountDto(
            count.Id,
            count.CountNumber,
            count.Title,
            count.CountDate,
            count.StorageLocation,
            count.CreatedBy,
            count.Status,
            count.PostedAtUtc,
            count.PostedBy,
            count.CountedLineCount,
            count.OpenLineCount,
            count.AbsoluteDifferenceQuantity,
            decimal.Round(
                lines.Sum(x => Math.Abs(x.DifferenceValue)),
                2,
                MidpointRounding.AwayFromZero),
            lines.Count(x => x.IsPriceOutdated),
            lines);
    }
}
