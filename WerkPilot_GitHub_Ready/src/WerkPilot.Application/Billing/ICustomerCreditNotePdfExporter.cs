namespace WerkPilot.Application.Billing;
public interface ICustomerCreditNotePdfExporter
{
    Task<string> ExportAsync(
        CustomerCreditNoteDocumentData document,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
