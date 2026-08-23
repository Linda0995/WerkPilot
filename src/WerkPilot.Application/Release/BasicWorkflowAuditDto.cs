namespace WerkPilot.Application.Release;

public sealed record BasicWorkflowAuditDto(
    DateTimeOffset EvaluatedAtUtc,
    int CustomerCount,
    int OfferCount,
    int AcceptedOfferCount,
    int WorkflowCount,
    int CompletedWorkflowCount,
    int IssueCount,
    int OrphanCount,
    IReadOnlyList<BasicWorkflowItemDto> Workflows,
    IReadOnlyList<BasicWorkflowOrphanDto> Orphans)
{
    public bool IsHealthy => IssueCount == 0 && OrphanCount == 0;
}
