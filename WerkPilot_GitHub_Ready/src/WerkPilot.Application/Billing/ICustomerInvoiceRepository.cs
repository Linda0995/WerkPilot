using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public interface ICustomerInvoiceRepository
{
    Task<IReadOnlyList<CustomerInvoice>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerInvoice?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken);
    Task AddAsync(CustomerInvoice invoice, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
