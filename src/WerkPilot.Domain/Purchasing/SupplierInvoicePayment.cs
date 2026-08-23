namespace WerkPilot.Domain.Purchasing;

public sealed class SupplierInvoicePayment
{
    private SupplierInvoicePayment() { }

    public SupplierInvoicePayment(
        decimal amount,
        DateOnly paymentDate,
        string? reference,
        string? createdBy)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Id = Guid.NewGuid();
        Amount = amount;
        PaymentDate = paymentDate;
        Reference = Clean(reference);
        CreatedBy = Clean(createdBy);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private init; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public string? Reference { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
