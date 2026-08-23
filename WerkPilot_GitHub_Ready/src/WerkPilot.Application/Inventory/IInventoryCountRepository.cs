using WerkPilot.Domain.Inventory;

namespace WerkPilot.Application.Inventory;

public interface IInventoryCountRepository
{
    Task<IReadOnlyList<InventoryCount>> GetAllAsync(CancellationToken cancellationToken);
    Task<InventoryCount?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(InventoryCount count, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
