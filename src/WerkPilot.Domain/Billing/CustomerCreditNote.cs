using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Billing;

public sealed class CustomerCreditNote : Entity
{
    private readonly List<CustomerCreditNoteLine> _lines = [];
    private CustomerCreditNote() { }

    public CustomerCreditNote(
        string creditNoteNumber,
        Guid customerInvoiceId,
        string customerInvoiceNumber,
        Guid customerId,
        string customerName,
        DateOnly creditNoteDate,
        string reason,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(creditNoteNumber))
            throw new ArgumentException("Gutschriftsnummer erforderlich.", nameof(creditNoteNumber));
        if (customerInvoiceId == Guid.Empty)
            throw new ArgumentException("Ausgangsrechnung erforderlich.", nameof(customerInvoiceId));
        if (string.IsNullOrWhiteSpace(customerInvoiceNumber))
            throw new ArgumentException("Rechnungsnummer erforderlich.", nameof(customerInvoiceNumber));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Kundenname erforderlich.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Korrekturgrund erforderlich.", nameof(reason));

        CreditNoteNumber = creditNoteNumber.Trim();
        CustomerInvoiceId = customerInvoiceId;
        CustomerInvoiceNumber = customerInvoiceNumber.Trim();
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        CreditNoteDate = creditNoteDate;
        Reason = reason.Trim();
        CreatedBy = Clean(createdBy);
        Status = CustomerCreditNoteStatus.Draft;
    }

    public string CreditNoteNumber { get; private set; } = string.Empty;
    public Guid CustomerInvoiceId { get; private set; }
    public string CustomerInvoiceNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateOnly CreditNoteDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? CreatedBy { get; private set; }
    public CustomerCreditNoteStatus Status { get; private set; }
    public DateTimeOffset? IssuedAtUtc { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public IReadOnlyCollection<CustomerCreditNoteLine> Lines => _lines.AsReadOnly();
    public decimal NetTotal => _lines.Sum(x => x.NetTotal);
    public decimal VatTotal => _lines.Sum(x => x.VatAmount);
    public decimal GrossTotal => NetTotal + VatTotal;

    public void AddLine(
        Guid? sourceInvoiceLineId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPriceNet,
        decimal vatRatePercent)
    {
        if (Status != CustomerCreditNoteStatus.Draft)
            throw new InvalidOperationException("Nur Gutschriftsentwürfe können bearbeitet werden.");

        _lines.Add(new CustomerCreditNoteLine(
            sourceInvoiceLineId,
            description,
            quantity,
            unit,
            unitPriceNet,
            vatRatePercent));

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Issue()
    {
        if (Status != CustomerCreditNoteStatus.Draft)
            throw new InvalidOperationException("Nur Entwürfe können ausgestellt werden.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Eine Gutschrift ohne Positionen kann nicht ausgestellt werden.");

        Status = CustomerCreditNoteStatus.Issued;
        IssuedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkApplied()
    {
        if (Status != CustomerCreditNoteStatus.Issued)
            throw new InvalidOperationException("Nur ausgestellte Gutschriften können verrechnet werden.");

        Status = CustomerCreditNoteStatus.Applied;
        AppliedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == CustomerCreditNoteStatus.Applied)
            throw new InvalidOperationException("Eine verrechnete Gutschrift kann nicht storniert werden.");

        Status = CustomerCreditNoteStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
