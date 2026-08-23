using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public interface ISupplierInvoiceRepository
{
    Task<IReadOnlyList<SupplierInvoice>> GetAllAsync(CancellationToken cancellationToken);
    Task<SupplierInvoice?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> InvoiceNumberExistsAsync(
        string supplierName,
        string invoiceNumber,
        CancellationToken cancellationToken);

    Task AddAsync(SupplierInvoice invoice, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
