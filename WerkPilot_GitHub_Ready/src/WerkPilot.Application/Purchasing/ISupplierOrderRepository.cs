using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public interface ISupplierOrderRepository
{
    Task<IReadOnlyList<SupplierOrder>> GetAllAsync(CancellationToken cancellationToken);
    Task<SupplierOrder?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(SupplierOrder order, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
