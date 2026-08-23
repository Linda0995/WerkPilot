using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Materials;
using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public sealed class InventoryService(
    IInventoryRepository repository,
    IMaterialRepository materialRepository,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        var result = new List<InventoryItemDto>();

        foreach (var item in items)
        {
            var material = await materialRepository.GetAsync(
                item.MaterialItemId,
                cancellationToken);

            if (material is null)
                continue;

            result.Add(Map(item, material.ArticleNumber, material.Description, material.Unit));
        }

        return result
            .OrderBy(x => x.IsBelowMinimum ? 0 : 1)
            .ThenBy(x => x.ArticleNumber)
            .ToArray();
    }

    public async Task<InventoryItemDto> CreateAsync(
        Guid materialItemId,
        string storageLocation,
        decimal minimumStock,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByMaterialIdAsync(
            materialItemId,
            cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException(
                "Für diesen Materialartikel existiert bereits ein Lagerbestand.");

        var material = await materialRepository.GetAsync(
            materialItemId,
            cancellationToken)
            ?? throw new InvalidOperationException("Materialartikel wurde nicht gefunden.");

        var item = new InventoryItem(
            materialItemId,
            storageLocation,
            minimumStock);

        await repository.AddItemAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(item, material.ArticleNumber, material.Description, material.Unit);
    }

    public async Task UpdateMasterDataAsync(
        Guid inventoryItemId,
        string storageLocation,
        decimal minimumStock,
        CancellationToken cancellationToken = default)
    {
        var item = await GetRequiredAsync(inventoryItemId, cancellationToken);
        item.UpdateMasterData(storageLocation, minimumStock);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task BookMovementAsync(
        Guid inventoryItemId,
        InventoryMovementType movementType,
        decimal quantity,
        string reason,
        Guid? projectId,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var item = await GetRequiredAsync(inventoryItemId, cancellationToken);

        item.ApplyMovement(movementType, quantity);

        var movement = new InventoryMovement(
            item.Id,
            movementType,
            quantity,
            reason,
            DateTimeOffset.UtcNow,
            projectId,
            reference,
            session.DisplayName);

        await repository.AddMovementAsync(movement, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "InventoryItem",
            item.Id,
            "MovementBooked",
            $"Lagerbewegung {movementType} mit Menge {quantity:N3} wurde gebucht.",
            cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovementDto>> GetMovementsAsync(
        Guid inventoryItemId,
        CancellationToken cancellationToken = default) =>
        (await repository.GetMovementsAsync(inventoryItemId, cancellationToken))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new InventoryMovementDto(
                x.Id,
                x.InventoryItemId,
                x.MovementType,
                x.Quantity,
                x.Reason,
                x.OccurredAtUtc,
                x.ProjectId,
                x.Reference,
                x.CreatedBy))
            .ToArray();

    private async Task<InventoryItem> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Lagerartikel wurde nicht gefunden.");

    private static InventoryItemDto Map(
        InventoryItem item,
        string articleNumber,
        string description,
        string unit) =>
        new(
            item.Id,
            item.MaterialItemId,
            articleNumber,
            description,
            unit,
            item.StorageLocation,
            item.QuantityOnHand,
            item.ReservedQuantity,
            item.AvailableQuantity,
            item.MinimumStock,
            item.IsBelowMinimum);
}
