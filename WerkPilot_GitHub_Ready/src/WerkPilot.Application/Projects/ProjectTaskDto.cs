using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Projects;

public sealed record ProjectTaskDto(
    Guid Id,
    int PositionNumber,
    string Title,
    Guid? AssignedUserId,
    string? AssignedTo,
    DateOnly? DueDate,
    ProjectTaskStatus Status,
    DateTimeOffset? CompletedAtUtc);
