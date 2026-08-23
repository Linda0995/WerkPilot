namespace WerkPilot.Application.Work;

public sealed record ReassignWorkResult(
    string SourceUserName,
    string TargetUserName,
    int CustomerFollowUpsTransferred,
    int ProjectTasksTransferred,
    int TotalTransferred);
