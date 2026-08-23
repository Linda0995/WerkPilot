using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public interface ICustomerFollowUpRepository
{
    Task<IReadOnlyList<CustomerFollowUp>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<CustomerFollowUp?> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        CustomerFollowUp followUp,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
