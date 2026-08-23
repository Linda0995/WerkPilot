using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<InventoryItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<InventoryItem?> GetByMaterialIdAsync(Guid materialItemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryMovement>> GetMovementsAsync(
        Guid inventoryItemId,
        CancellationToken cancellationToken);

    Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken);
    Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
