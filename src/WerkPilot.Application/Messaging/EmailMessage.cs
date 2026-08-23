namespace WerkPilot.Application.Messaging;

public sealed record EmailMessage(
    string Recipient,
    string Subject,
    string Body,
    IReadOnlyList<EmailAttachment> Attachments);
