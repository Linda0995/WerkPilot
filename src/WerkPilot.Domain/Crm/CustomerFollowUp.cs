using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Crm;

public sealed class CustomerFollowUp : Entity
{
    private CustomerFollowUp() { }

    public CustomerFollowUp(
        Guid customerId,
        string customerNumber,
        string customerName,
        string title,
        string? notes,
        DateTimeOffset dueAtUtc,
        CustomerFollowUpPriority priority,
        Guid? assignedUserId,
        string? assignedTo,
        string? createdBy)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(customerNumber))
            throw new ArgumentException("Kundennummer erforderlich.", nameof(customerNumber));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Kundenname erforderlich.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Aufgabentitel erforderlich.", nameof(title));

        CustomerId = customerId;
        CustomerNumber = customerNumber.Trim();
        CustomerName = customerName.Trim();
        Title = title.Trim();
        Notes = Clean(notes);
        DueAtUtc = dueAtUtc;
        Priority = priority;
        AssignedUserId = assignedUserId;
        AssignedTo = Clean(assignedTo);
        CreatedBy = Clean(createdBy);
        Status = CustomerFollowUpStatus.Open;
    }

    public Guid CustomerId { get; private set; }
    public string CustomerNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public CustomerFollowUpPriority Priority { get; private set; }
    public CustomerFollowUpStatus Status { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? CompletionNote { get; private set; }

    public bool IsOverdue(DateTimeOffset nowUtc) =>
        Status is CustomerFollowUpStatus.Open or CustomerFollowUpStatus.InProgress
        && DueAtUtc < nowUtc;

    public void Start()
    {
        if (Status != CustomerFollowUpStatus.Open)
            throw new InvalidOperationException("Nur offene Aufgaben können gestartet werden.");

        Status = CustomerFollowUpStatus.InProgress;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reschedule(
        DateTimeOffset dueAtUtc,
        CustomerFollowUpPriority priority,
        string? assignedTo,
        Guid? assignedUserId)
    {
        if (Status is CustomerFollowUpStatus.Completed or CustomerFollowUpStatus.Cancelled)
            throw new InvalidOperationException(
                "Abgeschlossene oder stornierte Aufgaben können nicht verschoben werden.");

        DueAtUtc = dueAtUtc;
        Priority = priority;
        AssignedTo = Clean(assignedTo);
        AssignedUserId = assignedUserId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }


    public void Reassign(
        Guid? assignedUserId,
        string? assignedTo)
    {
        if (Status is CustomerFollowUpStatus.Completed or CustomerFollowUpStatus.Cancelled)
            throw new InvalidOperationException(
                "Abgeschlossene oder stornierte Aufgaben können nicht neu zugewiesen werden.");

        AssignedUserId = assignedUserId;
        AssignedTo = Clean(assignedTo);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Complete(string? completionNote)
    {
        if (Status == CustomerFollowUpStatus.Completed)
            throw new InvalidOperationException("Die Aufgabe ist bereits abgeschlossen.");
        if (Status == CustomerFollowUpStatus.Cancelled)
            throw new InvalidOperationException("Eine stornierte Aufgabe kann nicht abgeschlossen werden.");

        Status = CustomerFollowUpStatus.Completed;
        CompletionNote = Clean(completionNote);
        CompletedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == CustomerFollowUpStatus.Completed)
            throw new InvalidOperationException("Eine abgeschlossene Aufgabe kann nicht storniert werden.");

        Status = CustomerFollowUpStatus.Cancelled;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
