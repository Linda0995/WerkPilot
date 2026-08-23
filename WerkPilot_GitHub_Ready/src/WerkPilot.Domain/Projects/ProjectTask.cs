namespace WerkPilot.Domain.Projects;

public sealed class ProjectTask
{
    private ProjectTask() { }

    public ProjectTask(
        int positionNumber,
        string title,
        Guid? assignedUserId,
        string? assignedTo,
        DateOnly? dueDate)
    {
        if (positionNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(positionNumber));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Aufgabentitel erforderlich.", nameof(title));

        Id = Guid.NewGuid();
        PositionNumber = positionNumber;
        Title = title.Trim();
        AssignedUserId = assignedUserId;
        AssignedTo = Clean(assignedTo);
        DueDate = dueDate;
        Status = ProjectTaskStatus.Open;
    }

    public Guid Id { get; private init; }
    public int PositionNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid? AssignedUserId { get; private set; }
    public string? AssignedTo { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public ProjectTaskStatus Status { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public void Update(
        string title,
        Guid? assignedUserId,
        string? assignedTo,
        DateOnly? dueDate,
        ProjectTaskStatus status)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Aufgabentitel erforderlich.", nameof(title));

        Title = title.Trim();
        AssignedUserId = assignedUserId;
        AssignedTo = Clean(assignedTo);
        DueDate = dueDate;
        SetStatus(status);
    }


    public void Reassign(
        Guid? assignedUserId,
        string? assignedTo)
    {
        if (Status == ProjectTaskStatus.Completed)
            throw new InvalidOperationException(
                "Eine abgeschlossene Projektaufgabe kann nicht neu zugewiesen werden.");

        AssignedUserId = assignedUserId;
        AssignedTo = Clean(assignedTo);
    }

    public void SetStatus(ProjectTaskStatus status)
    {
        Status = status;
        CompletedAtUtc = status == ProjectTaskStatus.Completed
            ? CompletedAtUtc ?? DateTimeOffset.UtcNow
            : null;
    }

    internal void SetPositionNumber(int positionNumber) =>
        PositionNumber = positionNumber;

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
