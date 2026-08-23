namespace WerkPilot.Application.Offers;

public interface IOfferDocumentExporter
{
    Task<string> ExportPdfAsync(
        OfferDocumentData document,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
