using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Purchasing;

public sealed record SupplierInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid SupplierOrderId,
    string SupplierOrderNumber,
    string SupplierName,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    string? CreatedBy,
    SupplierInvoiceStatus Status,
    string? ReviewNote,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAtUtc,
    DateTimeOffset? PaidAtUtc,
    decimal CashDiscountPercent,
    DateOnly? CashDiscountDueDate,
    decimal CashDiscountAmount,
    decimal DiscountedPayableAmount,
    decimal TotalNet,
    decimal PaidAmount,
    decimal OpenAmount,
    decimal TotalVariance,
    int WarningCount,
    int CriticalCount,
    SupplierInvoiceMatchStatus MatchStatus,
    IReadOnlyList<SupplierInvoiceLineDto> Lines,
    IReadOnlyList<SupplierInvoicePaymentDto> Payments);
