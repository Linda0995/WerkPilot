using WerkPilot.Application.Auditing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed class CustomerInteractionService(
    ICustomerInteractionRepository repository,
    ICustomerRepository customerRepository,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<CustomerInteractionDto>> GetForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        (await repository.GetForCustomerAsync(customerId, cancellationToken))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(Map)
            .ToArray();

    public async Task<IReadOnlyList<CustomerInteractionDto>> GetOpenFollowUpsAsync(
        DateOnly dueUntil,
        CancellationToken cancellationToken = default) =>
        (await repository.GetOpenFollowUpsAsync(dueUntil, cancellationToken))
            .OrderBy(x => x.FollowUpDate)
            .ThenBy(x => x.FollowUpOwner)
            .Select(Map)
            .ToArray();

    public async Task<CustomerInteractionDto> CreateAsync(
        Guid customerId,
        CustomerInteractionType interactionType,
        string subject,
        string notes,
        DateTimeOffset occurredAtUtc,
        string? contactPerson,
        DateOnly? followUpDate,
        string? followUpOwner,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

        var interaction = new CustomerInteraction(
            customerId,
            interactionType,
            subject,
            notes,
            occurredAtUtc,
            contactPerson,
            session.DisplayName,
            followUpDate,
            followUpOwner);

        customer.RegisterContact(occurredAtUtc);

        await repository.AddAsync(interaction, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "Customer",
            customerId,
            "InteractionCreated",
            $"CRM-Kontakt „{interaction.Subject}“ wurde erfasst.",
            cancellationToken);

        return Map(interaction);
    }

    public async Task UpdateAsync(
        Guid interactionId,
        CustomerInteractionType interactionType,
        string subject,
        string notes,
        DateTimeOffset occurredAtUtc,
        string? contactPerson,
        DateOnly? followUpDate,
        string? followUpOwner,
        CancellationToken cancellationToken = default)
    {
        var interaction = await GetRequiredAsync(interactionId, cancellationToken);
        interaction.Update(
            interactionType,
            subject,
            notes,
            occurredAtUtc,
            contactPerson,
            followUpDate,
            followUpOwner);

        var customer = await customerRepository.GetAsync(interaction.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

        customer.RegisterContact(occurredAtUtc);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleFollowUpCompletedAsync(
        Guid interactionId,
        CancellationToken cancellationToken = default)
    {
        var interaction = await GetRequiredAsync(interactionId, cancellationToken);
        interaction.SetFollowUpCompleted(!interaction.FollowUpCompleted);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<CustomerInteraction> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("CRM-Kontakt wurde nicht gefunden.");

    private static CustomerInteractionDto Map(CustomerInteraction x) => new(
        x.Id,
        x.CustomerId,
        x.InteractionType,
        x.Subject,
        x.Notes,
        x.OccurredAtUtc,
        x.ContactPerson,
        x.CreatedBy,
        x.FollowUpDate,
        x.FollowUpOwner,
        x.FollowUpCompleted,
        x.FollowUpCompletedAtUtc);
}
