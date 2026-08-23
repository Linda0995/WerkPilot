using Microsoft.EntityFrameworkCore;
using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class SupplierInvoiceRepository(WerkPilotDbContext dbContext)
    : ISupplierInvoiceRepository
{
    public async Task<IReadOnlyList<SupplierInvoice>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.SupplierInvoices
            .Include(x => x.Lines)
            .OrderByDescending(x => x.InvoiceDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<SupplierInvoice?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.SupplierInvoices
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> InvoiceNumberExistsAsync(
        string supplierName,
        string invoiceNumber,
        CancellationToken cancellationToken) =>
        dbContext.SupplierInvoices
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.SupplierName == supplierName &&
                     x.InvoiceNumber == invoiceNumber,
                cancellationToken);

    public Task AddAsync(
        SupplierInvoice invoice,
        CancellationToken cancellationToken) =>
        dbContext.SupplierInvoices.AddAsync(invoice, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
