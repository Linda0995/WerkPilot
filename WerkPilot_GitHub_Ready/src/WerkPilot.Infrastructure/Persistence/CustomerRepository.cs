using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Customers;
using WerkPilot.Domain.Customers;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CustomerRepository(WerkPilotDbContext dbContext) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchText,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        IQueryable<Customer> query = includeDeleted
            ? dbContext.Customers.IgnoreQueryFilters()
            : dbContext.Customers;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.DisplayName, $"%{term}%") ||
                EF.Functions.ILike(x.CustomerNumber, $"%{term}%") ||
                (x.VatId != null && EF.Functions.ILike(x.VatId, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.Phone != null && EF.Functions.ILike(x.Phone, $"%{term}%")) ||
                (x.BillingAddress != null &&
                    ((x.BillingAddress.City != null && EF.Functions.ILike(x.BillingAddress.City, $"%{term}%")) ||
                     (x.BillingAddress.PostalCode != null && EF.Functions.ILike(x.BillingAddress.PostalCode, $"%{term}%")))) ||
                x.Contacts.Any(c =>
                    EF.Functions.ILike(c.Label, $"%{term}%") ||
                    (c.Email != null && EF.Functions.ILike(c.Email, $"%{term}%")) ||
                    (c.Phone != null && EF.Functions.ILike(c.Phone, $"%{term}%"))));
        }

        return await query
            .Include(x => x.Contacts)
            .OrderByDescending(x => x.IsFavorite)
            .ThenBy(x => x.DisplayName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers
            .IgnoreQueryFilters()
            .Include(x => x.Contacts)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CustomerDuplicateDto>> FindDuplicatesAsync(
        string displayName,
        string? email,
        string? vatId,
        Guid? excludedCustomerId,
        CancellationToken cancellationToken)
    {
        var normalizedName = displayName.Trim().ToUpper();
        var normalizedEmail = Normalize(email);
        var normalizedVatId = Normalize(vatId);

        var query = dbContext.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => !excludedCustomerId.HasValue || x.Id != excludedCustomerId.Value);

        var candidates = await query
            .Where(x =>
                x.DisplayName.ToUpper() == normalizedName ||
                (normalizedEmail != null && x.Email != null && x.Email.ToUpper() == normalizedEmail) ||
                (normalizedVatId != null && x.VatId != null && x.VatId.ToUpper() == normalizedVatId))
            .Select(x => new
            {
                x.Id,
                x.CustomerNumber,
                x.DisplayName,
                NameMatch = x.DisplayName.ToUpper() == normalizedName,
                EmailMatch = normalizedEmail != null && x.Email != null && x.Email.ToUpper() == normalizedEmail,
                VatMatch = normalizedVatId != null && x.VatId != null && x.VatId.ToUpper() == normalizedVatId
            })
            .Take(20)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(x => new CustomerDuplicateDto(
                x.Id,
                x.CustomerNumber,
                x.DisplayName,
                x.VatMatch ? "gleiche UID/ATU" :
                x.EmailMatch ? "gleiche E-Mail-Adresse" :
                "gleicher Kundenname"))
            .ToArray();
    }

    public async Task<string> GetNextCustomerNumberAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"K-{year}-";
        var numbers = await dbContext.Customers
            .IgnoreQueryFilters()
            .Where(x => x.CustomerNumber.StartsWith(prefix))
            .Select(x => x.CustomerNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var number) ? number : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        dbContext.Customers.CountAsync(cancellationToken);

    public Task<int> CountFavoritesAsync(CancellationToken cancellationToken) =>
        dbContext.Customers.CountAsync(x => x.IsFavorite, cancellationToken);

    public Task AddAsync(Customer customer, CancellationToken cancellationToken) =>
        dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
