namespace WerkPilot.Application.Identity;

public sealed record UserAbsenceTransferResult(
    string SourceUserName,
    string SubstituteUserName,
    bool OnlyDueDuringAbsence,
    int CustomerFollowUpsTransferred,
    int ProjectTasksTransferred,
    int TotalTransferred);
