namespace WerkPilot.Application.Billing;
public interface ICustomerInvoicePdfExporter
{
    Task<string> ExportAsync(
        CustomerInvoiceDocumentData document,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
