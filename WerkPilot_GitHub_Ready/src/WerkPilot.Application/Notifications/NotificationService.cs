using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Application.Purchasing;
using WerkPilot.Domain.Notifications;
using WerkPilot.Domain.Identity;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;
using WerkPilot.Domain.Purchasing;

namespace WerkPilot.Application.Notifications;

public sealed class NotificationService(
    OfferService offerService,
    ProjectService projectService,
    WerkPilot.Application.Purchasing.PurchaseListService purchaseListService,
    CustomerInteractionService interactionService,
    CustomerFollowUpService customerFollowUpService,
    UserAbsenceService userAbsenceService,
    CustomerService customerService,
    INotificationReadRepository readRepository,
    SessionContext session)
{
    public async Task<IReadOnlyList<NotificationItem>> GetAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        if (!session.UserId.HasValue) return [];
        var readKeys = await readRepository.GetReadKeysAsync(session.UserId.Value, cancellationToken);
        var items = new List<NotificationItem>();

        foreach (var offer in await offerService.GetAllAsync(cancellationToken))
        {
            if (offer.Status == OfferStatus.Sent && offer.ValidUntil <= today.AddDays(7))
            {
                var overdue = offer.ValidUntil < today;
                var key = $"offer-expiry:{offer.Id}:{offer.ValidUntil:yyyyMMdd}";
                items.Add(new NotificationItem(key, overdue ? NotificationSeverity.Critical : NotificationSeverity.Warning,
                    "Angebot", overdue ? $"Angebot {offer.OfferNumber} ist abgelaufen" : $"Angebot {offer.OfferNumber} läuft bald ab",
                    offer.Title, offer.ValidUntil, offer.Id, readKeys.Contains(key)));
            }
        }

        foreach (var project in await projectService.GetAllAsync(cancellationToken))
        {
            if (project.Status is ProjectStatus.Completed or ProjectStatus.Cancelled) continue;
            foreach (var task in project.Tasks.Where(x => x.Status != ProjectTaskStatus.Completed && x.DueDate.HasValue && x.DueDate.Value <= today.AddDays(7)))
            {
                var overdue = task.DueDate!.Value < today;
                var key = $"project-task:{project.Id}:{task.Id}:{task.DueDate:yyyyMMdd}";
                items.Add(new NotificationItem(key, overdue ? NotificationSeverity.Critical : NotificationSeverity.Warning,
                    "Projektaufgabe", overdue ? $"Aufgabe in {project.ProjectNumber} ist überfällig" : $"Aufgabe in {project.ProjectNumber} wird fällig",
                    task.Title, task.DueDate, project.Id, readKeys.Contains(key)));
            }
        }

        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: false,
            cancellationToken);
        var customerNames = customers.ToDictionary(x => x.Id, x => x.DisplayName);

        foreach (var followUp in await interactionService.GetOpenFollowUpsAsync(
                     today.AddDays(7),
                     cancellationToken))
        {
            if (!followUp.FollowUpDate.HasValue)
                continue;

            var dueDate = followUp.FollowUpDate.Value;
            var overdue = dueDate < today;
            var customerName = customerNames.GetValueOrDefault(
                followUp.CustomerId,
                "Unbekannter Kunde");
            var key = $"crm-follow-up:{followUp.Id}:{dueDate:yyyyMMdd}";

            items.Add(new NotificationItem(
                key,
                overdue ? NotificationSeverity.Critical : NotificationSeverity.Warning,
                "CRM-Wiedervorlage",
                overdue
                    ? $"Wiedervorlage für {customerName} ist überfällig"
                    : $"Wiedervorlage für {customerName} wird fällig",
                string.IsNullOrWhiteSpace(followUp.FollowUpOwner)
                    ? followUp.Subject
                    : $"{followUp.Subject} · Verantwortlich: {followUp.FollowUpOwner}",
                dueDate,
                followUp.CustomerId,
                readKeys.Contains(key)));
        }


        foreach (var followUp in await customerFollowUpService.GetAllAsync(
                     DateTimeOffset.UtcNow,
                     cancellationToken))
        {
            var notification = CustomerFollowUpNotificationPolicy.Create(
                followUp,
                today,
                readKeys);

            if (notification is not null)
                items.Add(notification);
        }


        foreach (var absence in (await userAbsenceService.GetAllAsync(
                     today,
                     cancellationToken))
                 .Where(x =>
                     x.UserId == session.UserId.Value
                     && x.Status is UserAbsenceStatus.Planned or UserAbsenceStatus.Active
                     && x.StartDate <= today.AddDays(14)))
        {
            var key = $"user-absence:{absence.Id}:{absence.StartDate:yyyyMMdd}:{absence.EndDate:yyyyMMdd}";
            var active = absence.IsActiveToday;

            items.Add(new NotificationItem(
                key,
                absence.SubstituteUserId.HasValue
                    ? NotificationSeverity.Information
                    : NotificationSeverity.Warning,
                "Abwesenheit",
                active
                    ? $"Abwesenheit ist aktiv bis {absence.EndDate:dd.MM.yyyy}"
                    : $"Abwesenheit beginnt am {absence.StartDate:dd.MM.yyyy}",
                absence.SubstituteDisplayName is null
                    ? "Keine Vertretung eingetragen."
                    : $"Vertretung: {absence.SubstituteDisplayName}",
                absence.StartDate,
                absence.Id,
                readKeys.Contains(key)));
        }


        foreach (var list in await purchaseListService.GetAllAsync(cancellationToken))
        {
            if (list.Status is PurchaseListStatus.Draft or PurchaseListStatus.InProgress && list.OpenCount > 0)
            {
                var key = $"purchase-list:{list.Id}:{list.OpenCount}";
                items.Add(new NotificationItem(key, NotificationSeverity.Information, "Bestellliste",
                    $"{list.PurchaseListNumber}: {list.OpenCount} Position(en) offen", list.Title, null, list.Id, readKeys.Contains(key)));
            }
        }

        return items.OrderBy(x => x.IsRead).ThenByDescending(x => x.Severity).ThenBy(x => x.DueDate ?? DateOnly.MaxValue).ToArray();
    }

    public async Task MarkReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!session.UserId.HasValue) throw new InvalidOperationException("Keine aktive Sitzung.");
        var existing = await readRepository.GetAsync(session.UserId.Value, key, cancellationToken);
        if (existing is null) await readRepository.AddAsync(new NotificationReadState(session.UserId.Value, key), cancellationToken);
        else existing.MarkRead();
        await readRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        foreach (var key in keys.Distinct()) await MarkReadAsync(key, cancellationToken);
    }
}
