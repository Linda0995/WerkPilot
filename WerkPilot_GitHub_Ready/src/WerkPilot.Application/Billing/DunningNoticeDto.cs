using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed record DunningNoticeDto(
    Guid Id,
    string NoticeNumber,
    Guid CustomerInvoiceId,
    string CustomerInvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    DateOnly NoticeDate,
    DateOnly PaymentDeadline,
    DunningLevel Level,
    decimal PrincipalAmount,
    decimal FeeAmount,
    decimal InterestAmount,
    decimal AnnualInterestRatePercent,
    int OverdueDays,
    decimal TotalDue,
    string? CreatedBy,
    DunningNoticeStatus Status,
    DateTimeOffset? IssuedAtUtc);
