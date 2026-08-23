using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed record CustomerCreditNoteDto(
    Guid Id,
    string CreditNoteNumber,
    Guid CustomerInvoiceId,
    string CustomerInvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    DateOnly CreditNoteDate,
    string Reason,
    string? CreatedBy,
    CustomerCreditNoteStatus Status,
    DateTimeOffset? IssuedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    IReadOnlyList<CustomerCreditNoteLineDto> Lines);
