namespace WerkPilot.Application.Customers;

public sealed record CustomerCommunicationSummaryDto(
    Guid CustomerId,
    string CustomerNumber,
    string CustomerName,
    string? Email,
    DateTimeOffset? LastCommunicationAtUtc,
    int TotalCount,
    int SuccessfulCount,
    int FailedCount,
    int OfferCount,
    int InvoiceCount,
    int CreditNoteCount,
    int DunningCount,
    IReadOnlyList<CustomerCommunicationItemDto> Items);
