namespace WerkPilot.Application.Materials;

public sealed record MaterialImportResult(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    IReadOnlyList<string> Errors);
