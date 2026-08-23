namespace WerkPilot.Application.Purchasing;

public sealed record SupplierInvoiceLiquidityItemDto(
    Guid InvoiceId,
    string InvoiceNumber,
    string SupplierName,
    DateOnly DueDate,
    decimal OpenAmount,
    bool IsOverdue,
    int DaysUntilDue,
    bool CashDiscountAvailable,
    DateOnly? CashDiscountDueDate,
    decimal CashDiscountAmount,
    decimal DiscountedPayableAmount);
