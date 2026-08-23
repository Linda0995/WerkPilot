using WerkPilot.Application.Auditing;
using WerkPilot.Application.Billing;
using WerkPilot.Application.Messaging;
using WerkPilot.Application.Offers;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Customers;

public sealed class CustomerCommunicationService(
    CustomerService customerService,
    OfferService offerService,
    CustomerInvoiceService invoiceService,
    CustomerCreditNoteService creditNoteService,
    DunningNoticeService dunningNoticeService,
    DocumentEmailService documentEmailService,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<CustomerCommunicationSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: false,
            cancellationToken);

        var offers = await offerService.GetAllAsync(cancellationToken);
        var invoices = await invoiceService.GetAllAsync(
            DateOnly.FromDateTime(DateTime.Today),
            cancellationToken);
        var creditNotes = await creditNoteService.GetAllAsync(cancellationToken);
        var dunningNotices = await dunningNoticeService.GetAllAsync(cancellationToken);
        var dispatches = await documentEmailService.GetDispatchesAsync(cancellationToken);

        var invoiceCustomers = invoices.ToDictionary(x => x.Id, x => x.CustomerId);
        var creditNoteCustomers = creditNotes.ToDictionary(x => x.Id, x => x.CustomerId);
        var dunningCustomers = dunningNotices.ToDictionary(x => x.Id, x => x.CustomerId);

        var result = new List<CustomerCommunicationSummaryDto>(customers.Count);

        foreach (var customer in customers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = new List<CustomerCommunicationItemDto>();

            foreach (var offer in offers.Where(x => x.CustomerId == customer.Id))
            {
                var events = await auditTrail.GetForEntityAsync(
                    "Offer",
                    offer.Id,
                    maximumCount: 50,
                    cancellationToken);

                foreach (var entry in events.Where(x => x.Action == "EmailSent"))
                {
                    items.Add(new CustomerCommunicationItemDto(
                        CustomerCommunicationType.OfferEmail,
                        offer.Id,
                        offer.OfferNumber,
                        offer.Title,
                        customer.Email ?? string.Empty,
                        "Sent",
                        entry.OccurredAtUtc,
                        null));
                }
            }

            foreach (var dispatch in dispatches.Where(x =>
                         BelongsToCustomer(
                             customer.Id,
                             x,
                             invoiceCustomers,
                             creditNoteCustomers,
                             dunningCustomers)))
            {
                items.Add(new CustomerCommunicationItemDto(
                    MapType(dispatch.DocumentType),
                    dispatch.DocumentId,
                    dispatch.DocumentNumber,
                    dispatch.Subject,
                    dispatch.Recipient,
                    dispatch.Status.ToString(),
                    dispatch.SentAtUtc
                        ?? dispatch.LastAttemptAtUtc
                        ?? dispatch.CreatedAtUtc,
                    dispatch.ErrorMessage));
            }

            var ordered = items
                .OrderByDescending(x => x.OccurredAtUtc)
                .ToArray();

            result.Add(new CustomerCommunicationSummaryDto(
                customer.Id,
                customer.CustomerNumber,
                customer.DisplayName,
                customer.Email,
                ordered.FirstOrDefault()?.OccurredAtUtc,
                ordered.Length,
                ordered.Count(x => x.Status == "Sent"),
                ordered.Count(x => x.Status == "Failed"),
                ordered.Count(x => x.Type == CustomerCommunicationType.OfferEmail),
                ordered.Count(x => x.Type == CustomerCommunicationType.InvoiceEmail),
                ordered.Count(x => x.Type == CustomerCommunicationType.CreditNoteEmail),
                ordered.Count(x => x.Type == CustomerCommunicationType.DunningEmail),
                ordered));
        }

        return result
            .OrderByDescending(x => x.LastCommunicationAtUtc)
            .ThenBy(x => x.CustomerName)
            .ToArray();
    }

    private static bool BelongsToCustomer(
        Guid customerId,
        DocumentEmailDispatchDto dispatch,
        IReadOnlyDictionary<Guid, Guid> invoiceCustomers,
        IReadOnlyDictionary<Guid, Guid> creditNoteCustomers,
        IReadOnlyDictionary<Guid, Guid> dunningCustomers) =>
        dispatch.DocumentType switch
        {
            DocumentEmailType.CustomerInvoice =>
                invoiceCustomers.TryGetValue(dispatch.DocumentId, out var invoiceCustomerId)
                && invoiceCustomerId == customerId,

            DocumentEmailType.CustomerCreditNote =>
                creditNoteCustomers.TryGetValue(dispatch.DocumentId, out var creditCustomerId)
                && creditCustomerId == customerId,

            DocumentEmailType.DunningNotice =>
                dunningCustomers.TryGetValue(dispatch.DocumentId, out var dunningCustomerId)
                && dunningCustomerId == customerId,

            _ => false
        };

    private static CustomerCommunicationType MapType(DocumentEmailType type) => type switch
    {
        DocumentEmailType.CustomerInvoice => CustomerCommunicationType.InvoiceEmail,
        DocumentEmailType.CustomerCreditNote => CustomerCommunicationType.CreditNoteEmail,
        DocumentEmailType.DunningNotice => CustomerCommunicationType.DunningEmail,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
