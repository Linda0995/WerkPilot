using WerkPilot.Domain.Customers;

namespace WerkPilot.Application.Customers;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchText,
        bool includeDeleted,
        CancellationToken cancellationToken);

    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerDuplicateDto>> FindDuplicatesAsync(
        string displayName,
        string? email,
        string? vatId,
        Guid? excludedCustomerId,
        CancellationToken cancellationToken);

    Task<string> GetNextCustomerNumberAsync(int year, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<int> CountFavoritesAsync(CancellationToken cancellationToken);
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
