using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public interface IDunningNoticeRepository
{
    Task<IReadOnlyList<DunningNotice>> GetAllAsync(CancellationToken cancellationToken);
    Task<DunningNotice?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(DunningNotice notice, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
