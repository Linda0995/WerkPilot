using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Purchasing;

public sealed class SupplierInvoice : Entity
{
    private readonly List<SupplierInvoiceLine> _lines = [];
    private readonly List<SupplierInvoicePayment> _payments = [];
    private SupplierInvoice() { }

    public SupplierInvoice(
        string invoiceNumber,
        Guid supplierOrderId,
        string supplierName,
        DateOnly invoiceDate,
        DateOnly dueDate,
        string? createdBy,
        decimal cashDiscountPercent = 0m,
        DateOnly? cashDiscountDueDate = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Rechnungsnummer erforderlich.", nameof(invoiceNumber));
        if (supplierOrderId == Guid.Empty)
            throw new ArgumentException("Lieferantenbestellung erforderlich.", nameof(supplierOrderId));
        if (string.IsNullOrWhiteSpace(supplierName))
            throw new ArgumentException("Lieferant erforderlich.", nameof(supplierName));
        if (dueDate < invoiceDate)
            throw new ArgumentException("Fälligkeit darf nicht vor Rechnungsdatum liegen.", nameof(dueDate));
        if (cashDiscountPercent < 0m || cashDiscountPercent > 100m)
            throw new ArgumentOutOfRangeException(nameof(cashDiscountPercent));
        if (cashDiscountDueDate.HasValue && cashDiscountDueDate.Value > dueDate)
            throw new ArgumentException(
                "Die Skontofrist darf nicht nach der regulären Fälligkeit liegen.",
                nameof(cashDiscountDueDate));

        InvoiceNumber = invoiceNumber.Trim();
        SupplierOrderId = supplierOrderId;
        SupplierName = supplierName.Trim();
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        CreatedBy = Clean(createdBy);
        CashDiscountPercent = cashDiscountPercent;
        CashDiscountDueDate = cashDiscountDueDate;
        Status = SupplierInvoiceStatus.Draft;
    }

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid SupplierOrderId { get; private set; }
    public string SupplierName { get; private set; } = string.Empty;
    public DateOnly InvoiceDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string? CreatedBy { get; private set; }
    public SupplierInvoiceStatus Status { get; private set; }
    public string? ReviewNote { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAtUtc { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }
    public decimal CashDiscountPercent { get; private set; }
    public DateOnly? CashDiscountDueDate { get; private set; }
    public IReadOnlyCollection<SupplierInvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<SupplierInvoicePayment> Payments => _payments.AsReadOnly();
    public decimal TotalNet => _lines.Sum(x => x.LineTotalNet);
    public decimal PaidAmount => _payments.Sum(x => x.Amount);
    public decimal OpenAmount => Math.Max(0m, TotalNet - PaidAmount);
    public bool IsFullyPaid => OpenAmount == 0m;
    public decimal CashDiscountAmount => decimal.Round(
        TotalNet * CashDiscountPercent / 100m,
        2,
        MidpointRounding.AwayFromZero);
    public decimal DiscountedPayableAmount => Math.Max(0m, TotalNet - CashDiscountAmount);

    public void AddLine(
        Guid supplierOrderLineId,
        Guid materialItemId,
        string articleNumber,
        string description,
        decimal invoicedQuantity,
        decimal unitPriceNet)
    {
        EnsureEditable();

        if (_lines.Any(x => x.SupplierOrderLineId == supplierOrderLineId))
            throw new InvalidOperationException(
                "Die Bestellposition ist bereits in dieser Rechnung enthalten.");

        _lines.Add(new SupplierInvoiceLine(
            supplierOrderLineId,
            materialItemId,
            articleNumber,
            description,
            invoicedQuantity,
            unitPriceNet));

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateLine(Guid lineId, decimal invoicedQuantity, decimal unitPriceNet)
    {
        EnsureEditable();

        var line = _lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Rechnungsposition wurde nicht gefunden.");

        line.Update(invoicedQuantity, unitPriceNet);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SubmitForReview()
    {
        if (Status != SupplierInvoiceStatus.Draft)
            throw new InvalidOperationException("Nur Entwürfe können zur Prüfung eingereicht werden.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Eine Rechnung ohne Positionen kann nicht geprüft werden.");

        Status = SupplierInvoiceStatus.UnderReview;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Approve(string? approvedBy, string? reviewNote)
    {
        if (Status != SupplierInvoiceStatus.UnderReview)
            throw new InvalidOperationException("Nur Rechnungen in Prüfung können freigegeben werden.");

        Status = SupplierInvoiceStatus.Approved;
        ApprovedBy = Clean(approvedBy);
        ReviewNote = Clean(reviewNote);
        ApprovedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reject(string? reviewNote)
    {
        if (Status != SupplierInvoiceStatus.UnderReview)
            throw new InvalidOperationException("Nur Rechnungen in Prüfung können abgelehnt werden.");

        Status = SupplierInvoiceStatus.Rejected;
        ReviewNote = Clean(reviewNote);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RegisterPayment(
        decimal amount,
        DateOnly paymentDate,
        string? reference,
        string? createdBy)
    {
        if (Status is not SupplierInvoiceStatus.Approved and not SupplierInvoiceStatus.Paid)
            throw new InvalidOperationException(
                "Zahlungen können nur für freigegebene Rechnungen erfasst werden.");
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > OpenAmount)
            throw new InvalidOperationException(
                "Der Zahlungsbetrag überschreitet den offenen Rechnungsbetrag.");

        _payments.Add(new SupplierInvoicePayment(
            amount,
            paymentDate,
            reference,
            createdBy));

        if (IsFullyPaid)
        {
            Status = SupplierInvoiceStatus.Paid;
            PaidAtUtc = DateTimeOffset.UtcNow;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkPaid()
    {
        if (Status != SupplierInvoiceStatus.Approved)
            throw new InvalidOperationException("Nur freigegebene Rechnungen können bezahlt werden.");

        if (OpenAmount > 0m)
        {
            RegisterPayment(
                OpenAmount,
                DateOnly.FromDateTime(DateTime.Today),
                "Vollzahlung",
                null);
        }
    }

    public void Cancel()
    {
        if (Status is SupplierInvoiceStatus.Paid)
            throw new InvalidOperationException("Eine bezahlte Rechnung kann nicht storniert werden.");

        Status = SupplierInvoiceStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status is not SupplierInvoiceStatus.Draft and not SupplierInvoiceStatus.Rejected)
            throw new InvalidOperationException("Nur Entwürfe oder abgelehnte Rechnungen können bearbeitet werden.");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
