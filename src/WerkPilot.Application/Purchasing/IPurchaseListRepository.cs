using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public interface IPurchaseListRepository
{
    Task<IReadOnlyList<PurchaseList>> GetAllAsync(CancellationToken cancellationToken);
    Task<PurchaseList?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PurchaseList?> GetByOfferIdAsync(Guid offerId, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(PurchaseList purchaseList, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
