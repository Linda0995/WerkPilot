using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed record CreateCustomerFollowUpRequest(
    Guid CustomerId,
    string Title,
    string? Notes,
    DateTimeOffset DueAtUtc,
    CustomerFollowUpPriority Priority,
    Guid? AssignedUserId,
    string? AssignedTo);
