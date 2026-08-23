using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Inventory;
using WerkPilot.Application.Materials;
using WerkPilot.Domain.Inventory;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed class SupplierOrderService(
    ISupplierOrderRepository repository,
    IMaterialRepository materialRepository,
    IInventoryRepository inventoryRepository,
    InventoryService inventoryService,
    ReorderSuggestionService reorderSuggestionService,
    SessionContext session,
    ISupplierOrderCsvExporter csvExporter,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<SupplierOrderDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .OrderByDescending(x => x.OrderDate)
            .ThenByDescending(x => x.OrderNumber)
            .Select(Map)
            .ToArray();

    public async Task<SupplierOrderDto> CreateFromSuggestionsAsync(
        string supplierName,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string? supplierReference,
        CancellationToken cancellationToken = default)
    {
        var suggestions = (await reorderSuggestionService.GetAsync(cancellationToken))
            .Where(x => string.Equals(
                x.Supplier,
                supplierName,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (suggestions.Length == 0)
            throw new InvalidOperationException(
                "Für diesen Lieferanten liegen keine Nachbestellvorschläge vor.");

        var order = new SupplierOrder(
            await repository.GetNextNumberAsync(orderDate.Year, cancellationToken),
            supplierName,
            supplierReference,
            orderDate,
            expectedDeliveryDate,
            session.DisplayName);

        foreach (var item in suggestions)
        {
            order.AddLine(
                item.MaterialItemId,
                item.ArticleNumber,
                item.Description,
                item.Unit,
                item.SuggestedOrderQuantity,
                item.PurchasePrice);
        }

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "SupplierOrder",
            order.Id,
            "Created",
            $"Lieferantenbestellung {order.OrderNumber} wurde aus Nachbestellvorschlägen erzeugt.",
            cancellationToken);

        return Map(order);
    }

    public async Task MarkOrderedAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        order.MarkOrdered();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReceiveAsync(
        Guid orderId,
        Guid lineId,
        decimal quantity,
        string? deliveryReference,
        CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        var line = order.Lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Bestellposition wurde nicht gefunden.");

        order.Receive(lineId, quantity);

        var inventory = await inventoryRepository.GetByMaterialIdAsync(
            line.MaterialItemId,
            cancellationToken);

        if (inventory is null)
        {
            var material = await materialRepository.GetAsync(
                line.MaterialItemId,
                cancellationToken)
                ?? throw new InvalidOperationException("Materialartikel wurde nicht gefunden.");

            var created = await inventoryService.CreateAsync(
                material.Id,
                string.Empty,
                0m,
                cancellationToken);

            inventory = await inventoryRepository.GetAsync(
                created.Id,
                cancellationToken);
        }

        await inventoryService.BookMovementAsync(
            inventory!.Id,
            InventoryMovementType.Receipt,
            quantity,
            $"Wareneingang {order.OrderNumber}",
            null,
            deliveryReference ?? order.OrderNumber,
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "SupplierOrder",
            order.Id,
            "GoodsReceived",
            $"Wareneingang {quantity:N3} {line.Unit} für {line.ArticleNumber} wurde gebucht.",
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        order.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ExportCsvAsync(
        Guid orderId,
        CancellationToken cancellationToken = default) =>
        csvExporter.Export(Map(await GetRequiredAsync(orderId, cancellationToken)));

    private async Task<SupplierOrder> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Lieferantenbestellung wurde nicht gefunden.");

    private static SupplierOrderDto Map(SupplierOrder x) => new(
        x.Id,
        x.OrderNumber,
        x.SupplierName,
        x.SupplierReference,
        x.OrderDate,
        x.ExpectedDeliveryDate,
        x.CreatedBy,
        x.Status,
        x.OrderedAtUtc,
        x.ReceivedAtUtc,
        x.TotalNet,
        x.OpenQuantity,
        x.Lines
            .OrderBy(line => line.ArticleNumber)
            .Select(line => new SupplierOrderLineDto(
                line.Id,
                line.MaterialItemId,
                line.ArticleNumber,
                line.Description,
                line.Unit,
                line.OrderedQuantity,
                line.ReceivedQuantity,
                line.OpenQuantity,
                line.UnitPriceNet,
                line.LineTotalNet))
            .ToArray());
}
