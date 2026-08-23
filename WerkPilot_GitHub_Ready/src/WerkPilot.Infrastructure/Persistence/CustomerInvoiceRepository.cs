using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CustomerInvoiceRepository(WerkPilotDbContext dbContext)
    : ICustomerInvoiceRepository
{
    public async Task<IReadOnlyList<CustomerInvoice>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.CustomerInvoices
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .OrderByDescending(x => x.InvoiceDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<CustomerInvoice?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.CustomerInvoices
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<string> GetNextNumberAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var prefix = $"RE-{year}-";

        var numbers = await dbContext.CustomerInvoices
            .IgnoreQueryFilters()
            .Where(x => x.InvoiceNumber.StartsWith(prefix))
            .Select(x => x.InvoiceNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(
        CustomerInvoice invoice,
        CancellationToken cancellationToken) =>
        dbContext.CustomerInvoices.AddAsync(invoice, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
