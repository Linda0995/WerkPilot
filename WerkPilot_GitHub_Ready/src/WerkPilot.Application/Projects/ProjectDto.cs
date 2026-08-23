using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    string ProjectNumber,
    Guid CustomerId,
    Guid? SourceOfferId,
    string Title,
    string? Description,
    string? ProjectManager,
    DateOnly PlannedStart,
    DateOnly? PlannedEnd,
    ProjectStatus Status,
    int ProgressPercent,
    int OpenTaskCount,
    IReadOnlyList<ProjectTaskDto> Tasks);
