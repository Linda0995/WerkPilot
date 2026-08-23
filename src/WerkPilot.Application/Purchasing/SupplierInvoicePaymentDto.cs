namespace WerkPilot.Application.Purchasing;

public sealed record SupplierInvoicePaymentDto(
    Guid Id,
    decimal Amount,
    DateOnly PaymentDate,
    string? Reference,
    string? CreatedBy,
    DateTimeOffset CreatedAtUtc);
