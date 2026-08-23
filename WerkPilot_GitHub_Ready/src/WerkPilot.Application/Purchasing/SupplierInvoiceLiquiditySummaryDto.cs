namespace WerkPilot.Application.Purchasing;

public sealed record SupplierInvoiceLiquiditySummaryDto(
    decimal TotalOpenAmount,
    decimal OverdueAmount,
    decimal DueWithin7Days,
    decimal DueWithin14Days,
    decimal DueWithin30Days,
    decimal AvailableCashDiscount,
    int OpenInvoiceCount,
    int OverdueInvoiceCount,
    IReadOnlyList<SupplierInvoiceLiquidityItemDto> Items);
