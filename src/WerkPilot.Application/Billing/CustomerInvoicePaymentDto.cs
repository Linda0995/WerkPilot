namespace WerkPilot.Application.Billing;

public sealed record CustomerInvoicePaymentDto(
    Guid Id,
    decimal Amount,
    DateOnly PaymentDate,
    string? Reference,
    string? CreatedBy,
    DateTimeOffset CreatedAtUtc);
