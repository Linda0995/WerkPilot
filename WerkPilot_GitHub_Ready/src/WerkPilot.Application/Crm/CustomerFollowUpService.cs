using WerkPilot.Application.Auditing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Application.Crm;

public sealed class CustomerFollowUpService(
    ICustomerFollowUpRepository repository,
    CustomerService customerService,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<CustomerFollowUpDto>> GetAllAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.Status is CustomerFollowUpStatus.Completed or CustomerFollowUpStatus.Cancelled)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueAtUtc)
            .Select(x => Map(x, nowUtc))
            .ToArray();

    public async Task<IReadOnlyList<CustomerFollowUpDto>> GetForCustomerAsync(
        Guid customerId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Status is CustomerFollowUpStatus.Completed or CustomerFollowUpStatus.Cancelled)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueAtUtc)
            .Select(x => Map(x, nowUtc))
            .ToArray();

    public async Task<CustomerFollowUpDto> CreateAsync(
        CreateCustomerFollowUpRequest request,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: false,
            cancellationToken);

        var customer = customers.SingleOrDefault(x => x.Id == request.CustomerId)
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

        var followUp = new CustomerFollowUp(
            customer.Id,
            customer.CustomerNumber,
            customer.DisplayName,
            request.Title,
            request.Notes,
            request.DueAtUtc.ToUniversalTime(),
            request.Priority,
            request.AssignedUserId,
            request.AssignedTo,
            session.DisplayName);

        await repository.AddAsync(followUp, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerFollowUp",
            followUp.Id,
            "Created",
            $"Wiedervorlage für {customer.DisplayName} wurde erstellt.",
            cancellationToken);

        return Map(followUp, DateTimeOffset.UtcNow);
    }

    public async Task StartAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var followUp = await GetRequiredAsync(id, cancellationToken);
        followUp.Start();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RescheduleAsync(
        Guid id,
        RescheduleCustomerFollowUpRequest request,
        CancellationToken cancellationToken = default)
    {
        var followUp = await GetRequiredAsync(id, cancellationToken);

        followUp.Reschedule(
            request.DueAtUtc.ToUniversalTime(),
            request.Priority,
            request.AssignedTo,
            request.AssignedUserId);

        await repository.SaveChangesAsync(cancellationToken);
    }


    public async Task ReassignAsync(
        Guid id,
        Guid? assignedUserId,
        string? assignedTo,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Übergabegrund erforderlich.", nameof(reason));

        var followUp = await GetRequiredAsync(id, cancellationToken);
        var previous = followUp.AssignedTo ?? "Nicht zugewiesen";

        followUp.Reassign(assignedUserId, assignedTo);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerFollowUp",
            followUp.Id,
            "Reassigned",
            $"Verantwortung von „{previous}“ an „{assignedTo ?? "Nicht zugewiesen"}“ übergeben. Grund: {reason.Trim()}",
            cancellationToken);
    }

    public async Task CompleteAsync(
        Guid id,
        string? completionNote,
        CancellationToken cancellationToken = default)
    {
        var followUp = await GetRequiredAsync(id, cancellationToken);
        followUp.Complete(completionNote);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "CustomerFollowUp",
            followUp.Id,
            "Completed",
            $"Wiedervorlage „{followUp.Title}“ wurde abgeschlossen.",
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var followUp = await GetRequiredAsync(id, cancellationToken);
        followUp.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<CustomerFollowUp> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Wiedervorlage wurde nicht gefunden.");

    private static CustomerFollowUpDto Map(
        CustomerFollowUp x,
        DateTimeOffset nowUtc) => new(
            x.Id,
            x.CustomerId,
            x.CustomerNumber,
            x.CustomerName,
            x.Title,
            x.Notes,
            x.DueAtUtc,
            x.Priority,
            x.Status,
            x.AssignedUserId,
            x.AssignedTo,
            x.CreatedBy,
            x.CreatedAtUtc,
            x.CompletedAtUtc,
            x.CompletionNote,
            x.IsOverdue(nowUtc));
}
