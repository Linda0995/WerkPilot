namespace WerkPilot.Application.Messaging;

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    byte[] Content);
