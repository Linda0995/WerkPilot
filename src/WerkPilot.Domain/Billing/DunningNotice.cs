using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Billing;

public sealed class DunningNotice : Entity
{
    private DunningNotice() { }

    public DunningNotice(
        string noticeNumber,
        Guid customerInvoiceId,
        string customerInvoiceNumber,
        Guid customerId,
        string customerName,
        DateOnly noticeDate,
        DateOnly paymentDeadline,
        DunningLevel level,
        decimal principalAmount,
        decimal feeAmount,
        decimal interestAmount,
        decimal annualInterestRatePercent,
        int overdueDays,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(noticeNumber))
            throw new ArgumentException("Mahnnummer erforderlich.", nameof(noticeNumber));
        if (customerInvoiceId == Guid.Empty)
            throw new ArgumentException("Ausgangsrechnung erforderlich.", nameof(customerInvoiceId));
        if (principalAmount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(principalAmount));
        if (feeAmount < 0m || interestAmount < 0m)
            throw new ArgumentOutOfRangeException(nameof(feeAmount));
        if (annualInterestRatePercent < 0m)
            throw new ArgumentOutOfRangeException(nameof(annualInterestRatePercent));
        if (paymentDeadline < noticeDate)
            throw new ArgumentException("Zahlungsfrist darf nicht vor dem Mahndatum liegen.");

        NoticeNumber = noticeNumber.Trim();
        CustomerInvoiceId = customerInvoiceId;
        CustomerInvoiceNumber = customerInvoiceNumber.Trim();
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        NoticeDate = noticeDate;
        PaymentDeadline = paymentDeadline;
        Level = level;
        PrincipalAmount = principalAmount;
        FeeAmount = feeAmount;
        InterestAmount = interestAmount;
        AnnualInterestRatePercent = annualInterestRatePercent;
        OverdueDays = overdueDays;
        CreatedBy = Clean(createdBy);
        Status = DunningNoticeStatus.Draft;
    }

    public string NoticeNumber { get; private set; } = string.Empty;
    public Guid CustomerInvoiceId { get; private set; }
    public string CustomerInvoiceNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateOnly NoticeDate { get; private set; }
    public DateOnly PaymentDeadline { get; private set; }
    public DunningLevel Level { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal FeeAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal AnnualInterestRatePercent { get; private set; }
    public int OverdueDays { get; private set; }
    public string? CreatedBy { get; private set; }
    public DunningNoticeStatus Status { get; private set; }
    public DateTimeOffset? IssuedAtUtc { get; private set; }
    public decimal TotalDue => PrincipalAmount + FeeAmount + InterestAmount;

    public void Issue()
    {
        if (Status != DunningNoticeStatus.Draft)
            throw new InvalidOperationException("Nur Mahnentwürfe können ausgestellt werden.");

        Status = DunningNoticeStatus.Issued;
        IssuedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == DunningNoticeStatus.Issued)
            throw new InvalidOperationException(
                "Eine ausgestellte Mahnung kann nicht gelöscht, sondern nur durch einen Folgebeleg korrigiert werden.");

        Status = DunningNoticeStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
