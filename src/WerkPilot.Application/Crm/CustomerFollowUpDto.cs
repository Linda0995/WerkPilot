using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed record CustomerFollowUpDto(
    Guid Id,
    Guid CustomerId,
    string CustomerNumber,
    string CustomerName,
    string Title,
    string? Notes,
    DateTimeOffset DueAtUtc,
    CustomerFollowUpPriority Priority,
    CustomerFollowUpStatus Status,
    Guid? AssignedUserId,
    string? AssignedTo,
    string? CreatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? CompletionNote,
    bool IsOverdue);
