using System.Net.Mail;
using WerkPilot.Application.Auditing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Messaging;
using WerkPilot.Application.Settings;

namespace WerkPilot.Application.Offers;

public sealed class OfferEmailService(
    OfferService offerService,
    CustomerService customerService,
    CompanyProfileService companyProfileService,
    OfferDocumentService documentService,
    IEmailSender emailSender,
    IAuditTrail auditTrail)
{
    public async Task<OfferEmailPreview> CreatePreviewAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var offer = await offerService.GetAsync(offerId, cancellationToken);
        var company = await companyProfileService.GetAsync(cancellationToken);
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: true,
            cancellationToken);

        var customer = customers.SingleOrDefault(x => x.Id == offer.CustomerId)
            ?? throw new InvalidOperationException("Der Angebotskunde wurde nicht gefunden.");

        var recipient = customer.Email ?? string.Empty;
        var subject = ApplyTemplate(company.OfferEmailSubjectTemplate, offer, company, customer);
        var body = ApplyTemplate(company.OfferEmailBodyTemplate, offer, company, customer);

        return new OfferEmailPreview(recipient, subject, body);
    }

    public async Task SendAsync(
        SendOfferEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRecipient(request.Recipient);

        var offer = await offerService.GetAsync(request.OfferId, cancellationToken);
        if (offer.Status != WerkPilot.Domain.Offers.OfferStatus.Draft)
            throw new InvalidOperationException(
                "Nur Angebotsentwürfe können per E-Mail versendet werden.");

        var preview = await CreatePreviewAsync(request.OfferId, cancellationToken);
        var subject = string.IsNullOrWhiteSpace(request.SubjectOverride)
            ? preview.Subject
            : request.SubjectOverride.Trim();
        var body = string.IsNullOrWhiteSpace(request.BodyOverride)
            ? preview.Body
            : request.BodyOverride.Trim();

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Der E-Mail-Betreff ist erforderlich.");

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Der E-Mail-Text ist erforderlich.");

        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "WerkPilot",
            "OfferMail",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            var pdfPath = await documentService.ExportPdfAsync(
                request.OfferId,
                temporaryDirectory,
                cancellationToken);

            var attachment = new EmailAttachment(
                Path.GetFileName(pdfPath),
                "application/pdf",
                await File.ReadAllBytesAsync(pdfPath, cancellationToken));

            await emailSender.SendAsync(
                new EmailMessage(
                    request.Recipient.Trim(),
                    subject,
                    body,
                    [attachment]),
                cancellationToken);

            await offerService.MarkSentAsync(request.OfferId, cancellationToken);
            await auditTrail.WriteAsync(
                "Offer",
                request.OfferId,
                "EmailSent",
                $"Angebot wurde per E-Mail an {request.Recipient.Trim()} versendet.",
                cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(temporaryDirectory))
                    Directory.Delete(temporaryDirectory, recursive: true);
            }
            catch
            {
                // Temporäre Dateien werden beim nächsten System-Cleanup entfernt.
            }
        }
    }

    private static string ApplyTemplate(
        string template,
        OfferDetailsDto offer,
        CompanyProfileDto company,
        CustomerDto customer) =>
        template
            .Replace("{OfferNumber}", offer.OfferNumber, StringComparison.Ordinal)
            .Replace("{OfferTitle}", offer.Title, StringComparison.Ordinal)
            .Replace("{CompanyName}", company.CompanyName, StringComparison.Ordinal)
            .Replace("{CustomerName}", customer.DisplayName, StringComparison.Ordinal)
            .Replace("{ContactPerson}", customer.ContactPerson ?? customer.DisplayName, StringComparison.Ordinal)
            .Replace("{ValidUntil}", offer.ValidUntil.ToString("dd.MM.yyyy"), StringComparison.Ordinal)
            .Replace("{GrossTotal}", offer.GrossTotal.ToString("N2"), StringComparison.Ordinal);

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
            throw new ArgumentException("Die Empfängeradresse ist ungültig.", nameof(recipient), exception);
        }
    }
}
