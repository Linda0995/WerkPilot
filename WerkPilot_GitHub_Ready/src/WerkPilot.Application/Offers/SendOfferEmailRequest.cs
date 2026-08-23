namespace WerkPilot.Application.Offers;

public sealed record SendOfferEmailRequest(
    Guid OfferId,
    string Recipient,
    string? SubjectOverride,
    string? BodyOverride);
