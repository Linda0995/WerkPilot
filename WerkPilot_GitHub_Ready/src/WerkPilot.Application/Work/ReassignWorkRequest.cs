namespace WerkPilot.Application.Work;

public sealed record ReassignWorkRequest(
    Guid SourceUserId,
    Guid TargetUserId,
    string Reason,
    bool IncludeCustomerFollowUps,
    bool IncludeProjectTasks);
