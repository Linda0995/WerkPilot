namespace WerkPilot.Application.Projects;

public sealed record UpdateProjectRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    string? ProjectManager,
    DateOnly PlannedStart,
    DateOnly? PlannedEnd);
