namespace WerkPilot.Application.Messaging;

public sealed record DocumentEmailOutboxResult(
    int DueCount,
    int SentCount,
    int FailedCount,
    DateTimeOffset ProcessedAtUtc);
