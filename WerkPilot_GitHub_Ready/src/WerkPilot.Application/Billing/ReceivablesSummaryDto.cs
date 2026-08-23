namespace WerkPilot.Application.Billing;

public sealed record ReceivablesSummaryDto(
    decimal TotalOpenAmount,
    decimal OverdueAmount,
    decimal DueWithin7Days,
    decimal DueWithin14Days,
    decimal DueWithin30Days,
    int OpenInvoiceCount,
    int OverdueInvoiceCount,
    IReadOnlyList<CustomerInvoiceDto> Items);
