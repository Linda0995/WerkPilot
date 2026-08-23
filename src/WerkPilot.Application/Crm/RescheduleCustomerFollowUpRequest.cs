using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed record RescheduleCustomerFollowUpRequest(
    DateTimeOffset DueAtUtc,
    CustomerFollowUpPriority Priority,
    Guid? AssignedUserId,
    string? AssignedTo);
