using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public interface ICustomerCreditNoteRepository
{
    Task<IReadOnlyList<CustomerCreditNote>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerCreditNote?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(CustomerCreditNote creditNote, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
