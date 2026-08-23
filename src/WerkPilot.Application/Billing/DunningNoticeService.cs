using WerkPilot.Application.Auditing;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Settings;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Application.Billing;

public sealed class DunningNoticeService(
    IDunningNoticeRepository repository,
    ICustomerInvoiceRepository invoiceRepository,
    IDunningNoticePdfExporter pdfExporter,
    DocumentArchiveService archiveService,
    CompanyProfileService companyProfileService,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<DunningNoticeDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .OrderByDescending(x => x.NoticeDate)
            .ThenByDescending(x => x.NoticeNumber)
            .Select(Map)
            .ToArray();

    public async Task<DunningNoticeDto> CreateAsync(
        Guid customerInvoiceId,
        DateOnly noticeDate,
        int paymentTermDays,
        decimal feeAmount,
        decimal annualInterestRatePercent,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetAsync(
            customerInvoiceId,
            cancellationToken)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        if (!invoice.IsOverdue(noticeDate))
            throw new InvalidOperationException(
                "Eine Mahnung kann nur für eine überfällige offene Rechnung erstellt werden.");

        var overdueDays = noticeDate.DayNumber - invoice.DueDate.DayNumber;
        var interest = decimal.Round(
            invoice.OpenAmount *
            annualInterestRatePercent / 100m *
            overdueDays / 365m,
            2,
            MidpointRounding.AwayFromZero);

        var nextLevel = invoice.DunningLevel switch
        {
            DunningLevel.None => DunningLevel.Reminder,
            DunningLevel.Reminder => DunningLevel.FirstDunning,
            DunningLevel.FirstDunning => DunningLevel.SecondDunning,
            DunningLevel.SecondDunning => DunningLevel.FinalDunning,
            _ => DunningLevel.FinalDunning
        };

        var notice = new DunningNotice(
            await repository.GetNextNumberAsync(noticeDate.Year, cancellationToken),
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.CustomerName,
            noticeDate,
            noticeDate.AddDays(Math.Max(0, paymentTermDays)),
            nextLevel,
            invoice.OpenAmount,
            feeAmount,
            interest,
            annualInterestRatePercent,
            overdueDays,
            session.DisplayName);

        await repository.AddAsync(notice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "DunningNotice",
            notice.Id,
            "Created",
            $"Mahnung {notice.NoticeNumber} wurde als Entwurf erstellt.",
            cancellationToken);

        return Map(notice);
    }

    public async Task IssueAsync(
        Guid noticeId,
        CancellationToken cancellationToken = default)
    {
        var notice = await GetRequiredAsync(noticeId, cancellationToken);
        var invoice = await invoiceRepository.GetAsync(
            notice.CustomerInvoiceId,
            cancellationToken)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        notice.Issue();
        invoice.AdvanceDunning(notice.NoticeDate);

        await repository.SaveChangesAsync(cancellationToken);
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "DunningNotice",
            notice.Id,
            "Issued",
            $"Mahnung {notice.NoticeNumber} wurde ausgestellt.",
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid noticeId,
        CancellationToken cancellationToken = default)
    {
        var notice = await GetRequiredAsync(noticeId, cancellationToken);
        notice.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<DocumentArchiveResult> ExportPdfAsync(
        Guid noticeId,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var notice = Map(await GetRequiredAsync(noticeId, cancellationToken));
        var company = await companyProfileService.GetAsync(cancellationToken);

        var path = await pdfExporter.ExportAsync(
            notice,
            company,
            destinationDirectory,
            cancellationToken);

        return await archiveService.ArchiveAsync(
            path,
            "DunningNotice",
            notice.NoticeNumber,
            cancellationToken);
    }

    private async Task<DunningNotice> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Mahnung wurde nicht gefunden.");

    private static DunningNoticeDto Map(DunningNotice x) => new(
        x.Id,
        x.NoticeNumber,
        x.CustomerInvoiceId,
        x.CustomerInvoiceNumber,
        x.CustomerId,
        x.CustomerName,
        x.NoticeDate,
        x.PaymentDeadline,
        x.Level,
        x.PrincipalAmount,
        x.FeeAmount,
        x.InterestAmount,
        x.AnnualInterestRatePercent,
        x.OverdueDays,
        x.TotalDue,
        x.CreatedBy,
        x.Status,
        x.IssuedAtUtc);
}
