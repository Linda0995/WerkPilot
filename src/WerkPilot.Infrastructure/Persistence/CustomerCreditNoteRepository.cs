using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class CustomerCreditNoteRepository(WerkPilotDbContext dbContext)
    : ICustomerCreditNoteRepository
{
    public async Task<IReadOnlyList<CustomerCreditNote>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.CustomerCreditNotes
            .Include(x => x.Lines)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<CustomerCreditNote?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.CustomerCreditNotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<string> GetNextNumberAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"GS-{year}-";
        var numbers = await dbContext.CustomerCreditNotes
            .IgnoreQueryFilters()
            .Where(x => x.CreditNoteNumber.StartsWith(prefix))
            .Select(x => x.CreditNoteNumber)
            .ToListAsync(cancellationToken);

        var maximum = numbers
            .Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max();

        return $"{prefix}{maximum + 1:0000}";
    }

    public Task AddAsync(CustomerCreditNote creditNote, CancellationToken cancellationToken) =>
        dbContext.CustomerCreditNotes.AddAsync(creditNote, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
