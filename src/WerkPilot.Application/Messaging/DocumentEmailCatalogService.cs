using WerkPilot.Application.Billing;
using WerkPilot.Domain.Messaging;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Messaging;

public sealed class DocumentEmailCatalogService(
    CustomerInvoiceService invoiceService,
    CustomerCreditNoteService creditNoteService,
    DunningNoticeService dunningNoticeService)
{
    public async Task<IReadOnlyList<DocumentEmailDocumentOption>> GetAsync(
        DocumentEmailType type,
        CancellationToken cancellationToken = default)
    {
        return type switch
        {
            DocumentEmailType.CustomerInvoice =>
                (await invoiceService.GetAllAsync(
                    DateOnly.FromDateTime(DateTime.Today),
                    cancellationToken))
                .Where(x => x.Status is not CustomerInvoiceStatus.Draft
                    and not CustomerInvoiceStatus.Cancelled)
                .Select(x => new DocumentEmailDocumentOption(
                    type,
                    x.Id,
                    x.InvoiceNumber,
                    x.CustomerName,
                    x.InvoiceDate,
                    x.Status.ToString()))
                .OrderByDescending(x => x.DocumentDate)
                .ToArray(),

            DocumentEmailType.CustomerCreditNote =>
                (await creditNoteService.GetAllAsync(cancellationToken))
                .Where(x => x.Status is not CustomerCreditNoteStatus.Draft
                    and not CustomerCreditNoteStatus.Cancelled)
                .Select(x => new DocumentEmailDocumentOption(
                    type,
                    x.Id,
                    x.CreditNoteNumber,
                    x.CustomerName,
                    x.CreditNoteDate,
                    x.Status.ToString()))
                .OrderByDescending(x => x.DocumentDate)
                .ToArray(),

            DocumentEmailType.DunningNotice =>
                (await dunningNoticeService.GetAllAsync(cancellationToken))
                .Where(x => x.Status == DunningNoticeStatus.Issued)
                .Select(x => new DocumentEmailDocumentOption(
                    type,
                    x.Id,
                    x.NoticeNumber,
                    x.CustomerName,
                    x.NoticeDate,
                    x.Status.ToString()))
                .OrderByDescending(x => x.DocumentDate)
                .ToArray(),

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
