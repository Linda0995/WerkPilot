using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Billing;

public sealed class CustomerInvoice : Entity
{
    private readonly List<CustomerInvoiceLine> _lines = [];
    private readonly List<CustomerInvoicePayment> _payments = [];
    private decimal _creditedAmount;
    private CustomerInvoice() { }

    public CustomerInvoice(
        string invoiceNumber,
        Guid customerId,
        string customerName,
        DateOnly invoiceDate,
        DateOnly dueDate,
        Guid? sourceOfferId,
        Guid? sourceProjectId,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Rechnungsnummer erforderlich.", nameof(invoiceNumber));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Kundenname erforderlich.", nameof(customerName));
        if (dueDate < invoiceDate)
            throw new ArgumentException("Fälligkeit darf nicht vor Rechnungsdatum liegen.", nameof(dueDate));

        InvoiceNumber = invoiceNumber.Trim();
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        SourceOfferId = sourceOfferId;
        SourceProjectId = sourceProjectId;
        CreatedBy = Clean(createdBy);
        Status = CustomerInvoiceStatus.Draft;
        DunningLevel = DunningLevel.None;
    }

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateOnly InvoiceDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Guid? SourceOfferId { get; private set; }
    public Guid? SourceProjectId { get; private set; }
    public string? CreatedBy { get; private set; }
    public CustomerInvoiceStatus Status { get; private set; }
    public DunningLevel DunningLevel { get; private set; }
    public DateOnly? LastDunningDate { get; private set; }
    public DateTimeOffset? IssuedAtUtc { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }
    public IReadOnlyCollection<CustomerInvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<CustomerInvoicePayment> Payments => _payments.AsReadOnly();

    public decimal NetTotal => _lines.Sum(x => x.NetTotal);
    public decimal VatTotal => _lines.Sum(x => x.VatAmount);
    public decimal GrossTotal => NetTotal + VatTotal;
    public decimal PaidAmount => _payments.Sum(x => x.Amount);
    public decimal CreditedAmount => _creditedAmount;
    public decimal OpenAmount => Math.Max(0m, GrossTotal - PaidAmount - CreditedAmount);
    public bool IsOverdue(DateOnly today) =>
        Status is CustomerInvoiceStatus.Issued or CustomerInvoiceStatus.PartiallyPaid &&
        DueDate < today &&
        OpenAmount > 0m;

    public void AddLine(
        string description,
        decimal quantity,
        string unit,
        decimal unitPriceNet,
        decimal vatRatePercent)
    {
        if (Status != CustomerInvoiceStatus.Draft)
            throw new InvalidOperationException("Nur Rechnungsentwürfe können bearbeitet werden.");

        _lines.Add(new CustomerInvoiceLine(
            description,
            quantity,
            unit,
            unitPriceNet,
            vatRatePercent));

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Issue()
    {
        if (Status != CustomerInvoiceStatus.Draft)
            throw new InvalidOperationException("Nur Entwürfe können ausgestellt werden.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Eine Rechnung ohne Positionen kann nicht ausgestellt werden.");

        Status = CustomerInvoiceStatus.Issued;
        IssuedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RegisterPayment(
        decimal amount,
        DateOnly paymentDate,
        string? reference,
        string? createdBy)
    {
        if (Status is not CustomerInvoiceStatus.Issued and not CustomerInvoiceStatus.PartiallyPaid)
            throw new InvalidOperationException(
                "Zahlungen können nur für ausgestellte Rechnungen erfasst werden.");
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > OpenAmount)
            throw new InvalidOperationException(
                "Der Zahlungsbetrag überschreitet den offenen Rechnungsbetrag.");

        _payments.Add(new CustomerInvoicePayment(
            amount,
            paymentDate,
            reference,
            createdBy));

        if (OpenAmount == 0m)
        {
            Status = CustomerInvoiceStatus.Paid;
            PaidAtUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = CustomerInvoiceStatus.PartiallyPaid;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void AdvanceDunning(DateOnly date)
    {
        if (!IsOverdue(date))
            throw new InvalidOperationException(
                "Eine Mahnstufe kann nur für überfällige offene Rechnungen gesetzt werden.");

        DunningLevel = DunningLevel switch
        {
            DunningLevel.None => DunningLevel.Reminder,
            DunningLevel.Reminder => DunningLevel.FirstDunning,
            DunningLevel.FirstDunning => DunningLevel.SecondDunning,
            DunningLevel.SecondDunning => DunningLevel.FinalDunning,
            _ => DunningLevel.FinalDunning
        };

        LastDunningDate = date;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }


    public void ApplyCredit(decimal grossAmount)
    {
        if (Status is CustomerInvoiceStatus.Draft or CustomerInvoiceStatus.Cancelled)
            throw new InvalidOperationException(
                "Gutschriften können nur auf ausgestellte Rechnungen angewendet werden.");
        if (grossAmount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(grossAmount));
        if (grossAmount > OpenAmount)
            throw new InvalidOperationException(
                "Der Gutschriftsbetrag überschreitet den offenen Rechnungsbetrag.");

        _creditedAmount += grossAmount;

        if (OpenAmount == 0m)
        {
            Status = CustomerInvoiceStatus.Paid;
            PaidAtUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = CustomerInvoiceStatus.PartiallyPaid;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == CustomerInvoiceStatus.Paid)
            throw new InvalidOperationException("Eine bezahlte Rechnung kann nicht storniert werden.");

        Status = CustomerInvoiceStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
