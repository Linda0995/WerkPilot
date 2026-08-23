using WerkPilot.Domain.Workbench;

namespace WerkPilot.Application.Workbench;

public interface IWorkbenchRepository
{
    Task<IReadOnlyList<WorkbenchItem>> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<WorkbenchItem?> FindAsync(Guid userId, string itemType, Guid entityId, CancellationToken cancellationToken);
    Task<WorkbenchItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(WorkbenchItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
