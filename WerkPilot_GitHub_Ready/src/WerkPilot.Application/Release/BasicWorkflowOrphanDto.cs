namespace WerkPilot.Application.Release;

public sealed record BasicWorkflowOrphanDto(
    string EntityType,
    Guid EntityId,
    string Number,
    string Description,
    string Problem);
