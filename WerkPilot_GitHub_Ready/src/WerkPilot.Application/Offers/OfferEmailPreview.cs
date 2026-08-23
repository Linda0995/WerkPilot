namespace WerkPilot.Application.Offers;

public sealed record OfferEmailPreview(
    string Recipient,
    string Subject,
    string Body);
