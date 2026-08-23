using WerkPilot.Application.Auditing;
using WerkPilot.Application.Offers;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Projects;

public sealed class ProjectService(
    IProjectRepository repository,
    OfferService offerService,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .Select(Map)
            .ToArray();

    public async Task<ProjectDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Map(await GetRequiredAsync(id, cancellationToken));

    public async Task<ProjectDto> CreateFromAcceptedOfferAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetBySourceOfferIdAsync(offerId, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var offer = await offerService.GetAsync(offerId, cancellationToken);

        if (offer.Status != OfferStatus.Accepted)
            throw new InvalidOperationException(
                "Nur angenommene Angebote können in ein Projekt überführt werden.");

        var number = await repository.GetNextProjectNumberAsync(
            DateTime.Today.Year,
            cancellationToken);

        var project = new Project(
            number,
            offer.CustomerId,
            offer.Id,
            offer.Title,
            DateOnly.FromDateTime(DateTime.Today),
            null);

        project.AddTask("Projektstart vorbereiten", null, null, null);
        project.AddTask("Material und Termine prüfen", null, null, null);

        await repository.AddAsync(project, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "Project",
            project.Id,
            "CreatedFromOffer",
            $"Projekt {project.ProjectNumber} wurde aus {offer.OfferNumber} angelegt.",
            cancellationToken);

        return Map(project);
    }

    public async Task UpdateAsync(
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await GetRequiredAsync(request.ProjectId, cancellationToken);

        project.UpdateMasterData(
            request.Title,
            request.Description,
            request.ProjectManager,
            request.PlannedStart,
            request.PlannedEnd);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task AddTaskAsync(
        Guid projectId,
        string title,
        Guid? assignedUserId,
        string? assignedTo,
        DateOnly? dueDate,
        CancellationToken cancellationToken = default)
    {
        var project = await GetRequiredAsync(projectId, cancellationToken);
        var task = project.AddTask(title, assignedUserId, assignedTo, dueDate);

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "Project",
            project.Id,
            "TaskAdded",
            $"Aufgabe „{task.Title}“ wurde hinzugefügt.",
            cancellationToken);
    }

    public async Task UpdateTaskAsync(
        UpdateProjectTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await GetRequiredAsync(request.ProjectId, cancellationToken);

        project.UpdateTask(
            request.TaskId,
            request.Title,
            request.AssignedUserId,
            request.AssignedTo,
            request.DueDate,
            request.Status);

        await repository.SaveChangesAsync(cancellationToken);
    }


    public async Task ReassignTaskAsync(
        Guid projectId,
        Guid taskId,
        Guid? assignedUserId,
        string? assignedTo,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Übergabegrund erforderlich.", nameof(reason));

        var project = await GetRequiredAsync(projectId, cancellationToken);
        var task = project.Tasks.SingleOrDefault(x => x.Id == taskId)
            ?? throw new InvalidOperationException("Projektaufgabe wurde nicht gefunden.");

        var previous = task.AssignedTo ?? "Nicht zugewiesen";

        project.ReassignTask(
            taskId,
            assignedUserId,
            assignedTo);

        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "Project",
            project.Id,
            "TaskReassigned",
            $"Aufgabe „{task.Title}“ von „{previous}“ an „{assignedTo ?? "Nicht zugewiesen"}“ übergeben. Grund: {reason.Trim()}",
            cancellationToken);
    }

    public async Task RemoveTaskAsync(
        Guid projectId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetRequiredAsync(projectId, cancellationToken);
        project.RemoveTask(taskId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStatusAsync(
        Guid projectId,
        ProjectStatus status,
        CancellationToken cancellationToken = default)
    {
        var project = await GetRequiredAsync(projectId, cancellationToken);
        project.SetStatus(status);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "Project",
            project.Id,
            "StatusChanged",
            $"Projektstatus wurde auf {status} geändert.",
            cancellationToken);
    }

    public Task<int> CountActiveAsync(
        CancellationToken cancellationToken = default) =>
        repository.CountActiveAsync(cancellationToken);

    private async Task<Project> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Projekt wurde nicht gefunden.");

    private static ProjectDto Map(Project project) => new(
        project.Id,
        project.ProjectNumber,
        project.CustomerId,
        project.SourceOfferId,
        project.Title,
        project.Description,
        project.ProjectManager,
        project.PlannedStart,
        project.PlannedEnd,
        project.Status,
        project.ProgressPercent,
        project.OpenTaskCount,
        project.Tasks
            .OrderBy(x => x.PositionNumber)
            .Select(x => new ProjectTaskDto(
                x.Id,
                x.PositionNumber,
                x.Title,
                x.AssignedUserId,
                x.AssignedTo,
                x.DueDate,
                x.Status,
                x.CompletedAtUtc))
            .ToArray());
}
