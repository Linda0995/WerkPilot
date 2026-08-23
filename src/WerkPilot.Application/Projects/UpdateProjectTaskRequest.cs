using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Projects;

public sealed record UpdateProjectTaskRequest(
    Guid ProjectId,
    Guid TaskId,
    string Title,
    Guid? AssignedUserId,
    string? AssignedTo,
    DateOnly? DueDate,
    ProjectTaskStatus Status);
