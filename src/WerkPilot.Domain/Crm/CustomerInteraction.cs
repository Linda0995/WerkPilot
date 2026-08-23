using WerkPilot.Domain.Common;

namespace WerkPilot.Domain.Crm;

public sealed class CustomerInteraction : Entity
{
    private CustomerInteraction() { }

    public CustomerInteraction(
        Guid customerId,
        CustomerInteractionType interactionType,
        string subject,
        string notes,
        DateTimeOffset occurredAtUtc,
        string? contactPerson,
        string? createdBy,
        DateOnly? followUpDate,
        string? followUpOwner)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Kunde erforderlich.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Betreff erforderlich.", nameof(subject));
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notiz erforderlich.", nameof(notes));

        CustomerId = customerId;
        InteractionType = interactionType;
        Subject = subject.Trim();
        Notes = notes.Trim();
        OccurredAtUtc = occurredAtUtc;
        ContactPerson = Clean(contactPerson);
        CreatedBy = Clean(createdBy);
        FollowUpDate = followUpDate;
        FollowUpOwner = Clean(followUpOwner);
    }

    public Guid CustomerId { get; private set; }
    public CustomerInteractionType InteractionType { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateOnly? FollowUpDate { get; private set; }
    public string? FollowUpOwner { get; private set; }
    public bool FollowUpCompleted { get; private set; }
    public DateTimeOffset? FollowUpCompletedAtUtc { get; private set; }

    public void Update(
        CustomerInteractionType interactionType,
        string subject,
        string notes,
        DateTimeOffset occurredAtUtc,
        string? contactPerson,
        DateOnly? followUpDate,
        string? followUpOwner)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Betreff erforderlich.", nameof(subject));
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notiz erforderlich.", nameof(notes));

        InteractionType = interactionType;
        Subject = subject.Trim();
        Notes = notes.Trim();
        OccurredAtUtc = occurredAtUtc;
        ContactPerson = Clean(contactPerson);
        FollowUpDate = followUpDate;
        FollowUpOwner = Clean(followUpOwner);

        if (!followUpDate.HasValue)
        {
            FollowUpCompleted = false;
            FollowUpCompletedAtUtc = null;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetFollowUpCompleted(bool completed)
    {
        if (!FollowUpDate.HasValue && completed)
            throw new InvalidOperationException("Ohne Wiedervorlage kann nichts abgeschlossen werden.");

        FollowUpCompleted = completed;
        FollowUpCompletedAtUtc = completed ? DateTimeOffset.UtcNow : null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
