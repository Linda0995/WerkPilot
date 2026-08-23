using WerkPilot.Application.Customers;
using WerkPilot.Application.Settings;

namespace WerkPilot.Application.Offers;

public sealed class OfferDocumentService(
    OfferService offerService,
    CustomerService customerService,
    CompanyProfileService companyProfileService,
    IOfferDocumentExporter exporter)
{
    public async Task<string> ExportPdfAsync(
        Guid offerId,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var offer = await offerService.GetAsync(offerId, cancellationToken);
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        var customer = customers.SingleOrDefault(x => x.Id == offer.CustomerId)
            ?? throw new InvalidOperationException("Der dem Angebot zugeordnete Kunde wurde nicht gefunden.");

        var company = await companyProfileService.GetAsync(cancellationToken);

        var data = new OfferDocumentData(
            offer,
            company,
            customer.CustomerNumber,
            customer.DisplayName,
            customer.ContactPerson,
            customer.BillingStreet,
            customer.BillingPostalCode,
            customer.BillingCity,
            customer.BillingCountryCode,
            customer.VatId);

        return await exporter.ExportPdfAsync(data, destinationDirectory, cancellationToken);
    }
}
