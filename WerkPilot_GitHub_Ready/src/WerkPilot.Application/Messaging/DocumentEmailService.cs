using System.Net.Mail;
using WerkPilot.Application.Auditing;
using WerkPilot.Application.Billing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Settings;
using WerkPilot.Domain.Billing;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.Application.Messaging;

public sealed class DocumentEmailService(
    CustomerInvoiceService invoiceService,
    CustomerCreditNoteService creditNoteService,
    DunningNoticeService dunningService,
    CustomerService customerService,
    CompanyProfileService companyProfileService,
    DocumentEmailTemplateService templateService,
    IEmailSender emailSender,
    ISmtpDiagnostics smtpDiagnostics,
    IDocumentEmailDispatchRepository dispatchRepository,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<DocumentEmailPreview> CreatePreviewAsync(
        DocumentEmailType documentType,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var company = await companyProfileService.GetAsync(cancellationToken);

        var basePreview = documentType switch
        {
            DocumentEmailType.CustomerInvoice =>
                await CreateInvoicePreviewAsync(documentId, company, cancellationToken),
            DocumentEmailType.CustomerCreditNote =>
                await CreateCreditNotePreviewAsync(documentId, company, cancellationToken),
            DocumentEmailType.DunningNotice =>
                await CreateDunningPreviewAsync(documentId, company, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(documentType))
        };

        var template = await templateService.GetDefaultAsync(
            documentType,
            cancellationToken);

        if (template is null)
            return basePreview;

        var values = CreateTemplateValues(basePreview, company);

        return basePreview with
        {
            Subject = DocumentEmailTemplateService.Render(
                template.SubjectTemplate,
                values),
            Body = DocumentEmailTemplateService.Render(
                template.BodyTemplate,
                values)
        };
    }

    public async Task<Guid> SendAsync(
        SendDocumentEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRecipient(request.Recipient);

        var preview = await CreatePreviewAsync(
            request.DocumentType,
            request.DocumentId,
            cancellationToken);

        var subject = string.IsNullOrWhiteSpace(request.SubjectOverride)
            ? preview.Subject
            : request.SubjectOverride.Trim();

        var body = string.IsNullOrWhiteSpace(request.BodyOverride)
            ? preview.Body
            : request.BodyOverride.Trim();

        var dispatch = new DocumentEmailDispatch(
            request.DocumentType,
            request.DocumentId,
            preview.DocumentNumber,
            request.Recipient,
            subject,
            body,
            preview.AttachmentFileName,
            session.DisplayName);

        await dispatchRepository.AddAsync(dispatch, cancellationToken);
        await dispatchRepository.SaveChangesAsync(cancellationToken);

        await ExecuteDispatchAsync(dispatch, cancellationToken);
        return dispatch.Id;
    }

    public async Task RetryAsync(
        Guid dispatchId,
        CancellationToken cancellationToken = default)
    {
        var dispatch = await dispatchRepository.GetAsync(
            dispatchId,
            cancellationToken)
            ?? throw new InvalidOperationException("Versandvorgang wurde nicht gefunden.");

        if (dispatch.Status != DocumentEmailStatus.Failed)
            throw new InvalidOperationException(
                "Nur fehlgeschlagene Versandvorgänge können wiederholt werden.");

        await ExecuteDispatchAsync(dispatch, cancellationToken);
    }

    public async Task ScheduleRetryAsync(
        Guid dispatchId,
        DateTimeOffset retryAtUtc,
        CancellationToken cancellationToken = default)
    {
        var dispatch = await dispatchRepository.GetAsync(
            dispatchId,
            cancellationToken)
            ?? throw new InvalidOperationException("Versandvorgang wurde nicht gefunden.");

        dispatch.ScheduleRetry(retryAtUtc);
        await dispatchRepository.SaveChangesAsync(cancellationToken);
    }


    public Task<SmtpDiagnosticResult> TestSmtpAsync(
        CancellationToken cancellationToken = default) =>
        smtpDiagnostics.TestAsync(cancellationToken);

    public async Task<DocumentEmailOutboxResult> ProcessDueRetriesAsync(
        int maximumCount = 20,
        CancellationToken cancellationToken = default)
    {
        if (maximumCount is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(maximumCount));

        var due = await dispatchRepository.GetDueRetriesAsync(
            DateTimeOffset.UtcNow,
            maximumCount,
            cancellationToken);

        var sent = 0;
        var failed = 0;

        foreach (var dispatch in due)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ExecuteDispatchAsync(dispatch, cancellationToken);
                sent++;
            }
            catch
            {
                failed++;
            }
        }

        return new DocumentEmailOutboxResult(
            due.Count,
            sent,
            failed,
            DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<DocumentEmailDispatchDto>> GetDispatchesAsync(
        CancellationToken cancellationToken = default) =>
        (await dispatchRepository.GetAllAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapDispatch)
            .ToArray();

    private async Task ExecuteDispatchAsync(
        DocumentEmailDispatch dispatch,
        CancellationToken cancellationToken)
    {
        ValidateRecipient(dispatch.Recipient);

        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "WerkPilot",
            "DocumentMail",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempDirectory);
        dispatch.BeginAttempt();
        await dispatchRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var pdfPath = await ExportPdfAsync(
                dispatch.DocumentType,
                dispatch.DocumentId,
                tempDirectory,
                cancellationToken);

            var attachment = new EmailAttachment(
                Path.GetFileName(pdfPath),
                "application/pdf",
                await File.ReadAllBytesAsync(pdfPath, cancellationToken));

            await emailSender.SendAsync(
                new EmailMessage(
                    dispatch.Recipient,
                    dispatch.Subject,
                    dispatch.Body,
                    [attachment]),
                cancellationToken);

            dispatch.MarkSent();
            await dispatchRepository.SaveChangesAsync(cancellationToken);

            await auditTrail.WriteAsync(
                dispatch.DocumentType.ToString(),
                dispatch.DocumentId,
                "EmailSent",
                $"{dispatch.DocumentNumber} wurde per E-Mail an {dispatch.Recipient} versendet.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            var delayMinutes = Math.Min(60, 5 * Math.Max(1, dispatch.AttemptCount));
            dispatch.MarkFailed(
                exception.Message,
                DateTimeOffset.UtcNow.AddMinutes(delayMinutes));

            await dispatchRepository.SaveChangesAsync(cancellationToken);
            throw;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, recursive: true);
            }
            catch
            {
                // Temporäre Dateien werden vom Betriebssystem bereinigt.
            }
        }
    }

    private async Task<DocumentEmailPreview> CreateInvoicePreviewAsync(
        Guid id,
        CompanyProfileDto company,
        CancellationToken cancellationToken)
    {
        var invoice = (await invoiceService.GetAllAsync(
            DateOnly.FromDateTime(DateTime.Today),
            cancellationToken)).SingleOrDefault(x => x.Id == id)
            ?? throw new InvalidOperationException("Ausgangsrechnung wurde nicht gefunden.");

        return new DocumentEmailPreview(
            DocumentEmailType.CustomerInvoice,
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerName,
            await ResolveCustomerEmailAsync(invoice.CustomerId, cancellationToken),
            $"Rechnung {invoice.InvoiceNumber} – {company.CompanyName}",
            $"Sehr geehrte Damen und Herren,\n\n"
            + $"anbei erhalten Sie unsere Rechnung {invoice.InvoiceNumber} "
            + $"über {invoice.GrossTotal:N2} EUR.\n"
            + $"Der offene Betrag ist bis {invoice.DueDate:dd.MM.yyyy} fällig.\n\n"
            + $"Mit freundlichen Grüßen\n{company.CompanyName}",
            $"{invoice.InvoiceNumber}.pdf");
    }

    private async Task<DocumentEmailPreview> CreateCreditNotePreviewAsync(
        Guid id,
        CompanyProfileDto company,
        CancellationToken cancellationToken)
    {
        var note = (await creditNoteService.GetAllAsync(cancellationToken))
            .SingleOrDefault(x => x.Id == id)
            ?? throw new InvalidOperationException("Gutschrift wurde nicht gefunden.");

        return new DocumentEmailPreview(
            DocumentEmailType.CustomerCreditNote,
            note.Id,
            note.CreditNoteNumber,
            note.CustomerName,
            await ResolveCustomerEmailAsync(note.CustomerId, cancellationToken),
            $"Gutschrift {note.CreditNoteNumber} – {company.CompanyName}",
            $"Sehr geehrte Damen und Herren,\n\n"
            + $"anbei erhalten Sie unsere Gutschrift {note.CreditNoteNumber} "
            + $"zur Rechnung {note.CustomerInvoiceNumber}.\n\n"
            + $"Mit freundlichen Grüßen\n{company.CompanyName}",
            $"{note.CreditNoteNumber}.pdf");
    }

    private async Task<DocumentEmailPreview> CreateDunningPreviewAsync(
        Guid id,
        CompanyProfileDto company,
        CancellationToken cancellationToken)
    {
        var notice = (await dunningService.GetAllAsync(cancellationToken))
            .SingleOrDefault(x => x.Id == id)
            ?? throw new InvalidOperationException("Mahnung wurde nicht gefunden.");

        var title = notice.Level switch
        {
            DunningLevel.Reminder => "Zahlungserinnerung",
            DunningLevel.FirstDunning => "1. Mahnung",
            DunningLevel.SecondDunning => "2. Mahnung",
            DunningLevel.FinalDunning => "Letzte Mahnung",
            _ => "Mahnung"
        };

        return new DocumentEmailPreview(
            DocumentEmailType.DunningNotice,
            notice.Id,
            notice.NoticeNumber,
            notice.CustomerName,
            await ResolveCustomerEmailAsync(notice.CustomerId, cancellationToken),
            $"{title} zu Rechnung {notice.CustomerInvoiceNumber}",
            $"Sehr geehrte Damen und Herren,\n\n"
            + $"anbei erhalten Sie unsere {title.ToLowerInvariant()} "
            + $"zur Rechnung {notice.CustomerInvoiceNumber}.\n"
            + $"Die Gesamtforderung von {notice.TotalDue:N2} EUR ist "
            + $"bis {notice.PaymentDeadline:dd.MM.yyyy} zu begleichen.\n\n"
            + $"Mit freundlichen Grüßen\n{company.CompanyName}",
            $"{notice.NoticeNumber}.pdf");
    }

    private async Task<string> ResolveCustomerEmailAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        return customers.SingleOrDefault(x => x.Id == customerId)?.Email
            ?? string.Empty;
    }

    private Task<string> ExportPdfAsync(
        DocumentEmailType documentType,
        Guid documentId,
        string directory,
        CancellationToken cancellationToken) =>
        documentType switch
        {
            DocumentEmailType.CustomerInvoice =>
                ExportInvoicePdfAsync(documentId, directory, cancellationToken),
            DocumentEmailType.CustomerCreditNote =>
                ExportCreditNotePdfAsync(documentId, directory, cancellationToken),
            DocumentEmailType.DunningNotice =>
                ExportDunningPdfAsync(documentId, directory, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(documentType))
        };

    private async Task<string> ExportInvoicePdfAsync(
        Guid id,
        string directory,
        CancellationToken cancellationToken) =>
        (await invoiceService.ExportPdfAsync(id, directory, cancellationToken)).PdfPath;

    private async Task<string> ExportCreditNotePdfAsync(
        Guid id,
        string directory,
        CancellationToken cancellationToken) =>
        (await creditNoteService.ExportPdfAsync(id, directory, cancellationToken)).PdfPath;

    private async Task<string> ExportDunningPdfAsync(
        Guid id,
        string directory,
        CancellationToken cancellationToken) =>
        (await dunningService.ExportPdfAsync(id, directory, cancellationToken)).PdfPath;

    private static IReadOnlyDictionary<string, string> CreateTemplateValues(
        DocumentEmailPreview preview,
        CompanyProfileDto company) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Belegnummer"] = preview.DocumentNumber,
            ["Kundenname"] = preview.CustomerName,
            ["Firmenname"] = company.CompanyName,
            ["Anhang"] = preview.AttachmentFileName
        };

    private static DocumentEmailDispatchDto MapDispatch(DocumentEmailDispatch x) => new(
        x.Id,
        x.DocumentType,
        x.DocumentId,
        x.DocumentNumber,
        x.Recipient,
        x.Subject,
        x.AttachmentFileName,
        x.CreatedBy,
        x.Status,
        x.CreatedAtUtc,
        x.SentAtUtc,
        x.ErrorMessage,
        x.AttemptCount,
        x.LastAttemptAtUtc,
        x.NextRetryAtUtc);

    private static void ValidateRecipient(string recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Eine Empfängeradresse ist erforderlich.");

        try
        {
            _ = new MailAddress(recipient.Trim());
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                "Die Empfängeradresse ist ungültig.",
                nameof(recipient),
                exception);
        }
    }
}
