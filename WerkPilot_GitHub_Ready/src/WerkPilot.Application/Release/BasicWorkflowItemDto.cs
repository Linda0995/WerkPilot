namespace WerkPilot.Application.Release;

public sealed record BasicWorkflowItemDto(
    Guid OfferId,
    string OfferNumber,
    Guid CustomerId,
    string CustomerName,
    string Title,
    string OfferStatus,
    bool HasCalculation,
    bool HasProject,
    string? ProjectNumber,
    bool HasInvoice,
    string? InvoiceNumber,
    string? InvoiceStatus,
    decimal InvoiceOpenAmount,
    bool HasPayment,
    bool HasDunning,
    string Stage,
    int CompletionPercent,
    bool HasIssue,
    string? IssueText);
