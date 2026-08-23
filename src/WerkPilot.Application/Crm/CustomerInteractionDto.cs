using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed record CustomerInteractionDto(
    Guid Id,
    Guid CustomerId,
    CustomerInteractionType InteractionType,
    string Subject,
    string Notes,
    DateTimeOffset OccurredAtUtc,
    string? ContactPerson,
    string? CreatedBy,
    DateOnly? FollowUpDate,
    string? FollowUpOwner,
    bool FollowUpCompleted,
    DateTimeOffset? FollowUpCompletedAtUtc);
