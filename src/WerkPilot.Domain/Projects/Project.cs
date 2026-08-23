using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Projects;

public sealed class Project : Entity
{
    private readonly List<ProjectTask> _tasks = [];
    private Project() { }

    public Project(
        string projectNumber,
        Guid customerId,
        Guid? sourceOfferId,
        string title,
        DateOnly plannedStart,
        DateOnly? plannedEnd)
    {
        if (string.IsNullOrWhiteSpace(projectNumber))
            throw new ArgumentException("Projektnummer erforderlich.", nameof(projectNumber));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Projekttitel erforderlich.", nameof(title));
        if (plannedEnd.HasValue && plannedEnd.Value < plannedStart)
            throw new ArgumentException("Das geplante Ende liegt vor dem Projektstart.", nameof(plannedEnd));

        ProjectNumber = projectNumber.Trim();
        CustomerId = customerId;
        SourceOfferId = sourceOfferId;
        Title = title.Trim();
        PlannedStart = plannedStart;
        PlannedEnd = plannedEnd;
        Status = ProjectStatus.Planned;
    }

    public string ProjectNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid? SourceOfferId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ProjectManager { get; private set; }
    public DateOnly PlannedStart { get; private set; }
    public DateOnly? PlannedEnd { get; private set; }
    public ProjectStatus Status { get; private set; }
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    public int ProgressPercent =>
        _tasks.Count == 0
            ? Status == ProjectStatus.Completed ? 100 : 0
            : (int)Math.Round(
                _tasks.Count(x => x.Status == ProjectTaskStatus.Completed) * 100m / _tasks.Count,
                MidpointRounding.AwayFromZero);

    public int OpenTaskCount =>
        _tasks.Count(x => x.Status != ProjectTaskStatus.Completed);

    public void UpdateMasterData(
        string title,
        string? description,
        string? projectManager,
        DateOnly plannedStart,
        DateOnly? plannedEnd)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Projekttitel erforderlich.", nameof(title));
        if (plannedEnd.HasValue && plannedEnd.Value < plannedStart)
            throw new ArgumentException("Das geplante Ende liegt vor dem Projektstart.", nameof(plannedEnd));

        Title = title.Trim();
        Description = Clean(description);
        ProjectManager = Clean(projectManager);
        PlannedStart = plannedStart;
        PlannedEnd = plannedEnd;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public ProjectTask AddTask(
        string title,
        Guid? assignedUserId,
        string? assignedTo,
        DateOnly? dueDate)
    {
        EnsureEditable();

        var task = new ProjectTask(
            _tasks.Count + 1,
            title,
            assignedUserId,
            assignedTo,
            dueDate);

        _tasks.Add(task);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return task;
    }

    public void UpdateTask(
        Guid taskId,
        string title,
        Guid? assignedUserId,
        string? assignedTo,
        DateOnly? dueDate,
        ProjectTaskStatus status)
    {
        EnsureEditable();

        var task = GetTask(taskId);
        task.Update(title, assignedUserId, assignedTo, dueDate, status);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }


    public void ReassignTask(
        Guid taskId,
        Guid? assignedUserId,
        string? assignedTo)
    {
        EnsureEditable();

        var task = GetTask(taskId);
        task.Reassign(assignedUserId, assignedTo);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemoveTask(Guid taskId)
    {
        EnsureEditable();

        var task = GetTask(taskId);
        _tasks.Remove(task);

        for (var index = 0; index < _tasks.Count; index++)
            _tasks[index].SetPositionNumber(index + 1);

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetStatus(ProjectStatus status)
    {
        if (Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException("Ein storniertes Projekt kann nicht mehr geändert werden.");

        if (status == ProjectStatus.Completed &&
            _tasks.Any(x => x.Status != ProjectTaskStatus.Completed))
            throw new InvalidOperationException(
                "Ein Projekt mit offenen Aufgaben kann nicht abgeschlossen werden.");

        Status = status;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private ProjectTask GetTask(Guid taskId) =>
        _tasks.SingleOrDefault(x => x.Id == taskId)
        ?? throw new InvalidOperationException("Projektaufgabe wurde nicht gefunden.");

    private void EnsureEditable()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
            throw new InvalidOperationException(
                "Abgeschlossene oder stornierte Projekte können nicht bearbeitet werden.");
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
