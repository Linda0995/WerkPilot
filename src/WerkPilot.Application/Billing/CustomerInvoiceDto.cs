using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed record CustomerInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    Guid? SourceOfferId,
    Guid? SourceProjectId,
    string? CreatedBy,
    CustomerInvoiceStatus Status,
    DunningLevel DunningLevel,
    DateOnly? LastDunningDate,
    DateTimeOffset? IssuedAtUtc,
    DateTimeOffset? PaidAtUtc,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    decimal PaidAmount,
    decimal CreditedAmount,
    decimal OpenAmount,
    bool IsOverdue,
    int DaysOverdue,
    IReadOnlyList<CustomerInvoiceLineDto> Lines,
    IReadOnlyList<CustomerInvoicePaymentDto> Payments);
