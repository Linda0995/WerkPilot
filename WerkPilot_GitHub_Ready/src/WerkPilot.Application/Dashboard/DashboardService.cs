using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Dashboard;

public sealed class DashboardService(
    OfferService offerService,
    ProjectService projectService,
    CustomerInteractionService interactionService,
    CustomerFollowUpService customerFollowUpService,
    CustomerCommunicationService customerCommunicationService,
    CustomerService customerService)
{
    public async Task<DashboardDto> GetAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var offers = await offerService.GetAllAsync(cancellationToken);
        var projects = await projectService.GetAllAsync(cancellationToken);
        var customers = await customerService.SearchAsync(
            null,
            includeDeleted: false,
            cancellationToken);

        var customerNames = customers.ToDictionary(x => x.Id, x => x.DisplayName);

        var openFollowUps = await interactionService.GetOpenFollowUpsAsync(
            today.AddDays(30),
            cancellationToken);

        var crmFollowUps = openFollowUps
            .Where(x => x.FollowUpDate.HasValue)
            .OrderBy(x => x.FollowUpDate)
            .ThenBy(x => x.FollowUpOwner)
            .Select(x => new DashboardCrmFollowUpItem(
                x.Id,
                x.CustomerId,
                customerNames.GetValueOrDefault(x.CustomerId, "Unbekannter Kunde"),
                x.Subject,
                x.FollowUpOwner,
                x.FollowUpDate!.Value,
                x.FollowUpDate.Value < today))
            .Take(12)
            .ToArray();

        var nowUtc = DateTimeOffset.UtcNow;
        var localToday = DateOnly.FromDateTime(DateTime.Now);

        var customerFollowUpsAll = await customerFollowUpService.GetAllAsync(
            nowUtc,
            cancellationToken);

        var openCustomerFollowUps = customerFollowUpsAll
            .Where(x => x.Status is CustomerFollowUpStatus.Open
                or CustomerFollowUpStatus.InProgress)
            .ToArray();

        var customerFollowUps = openCustomerFollowUps
            .OrderByDescending(x => x.IsOverdue)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueAtUtc)
            .Take(12)
            .Select(x =>
            {
                var dueLocal = x.DueAtUtc.ToLocalTime();
                return new DashboardCustomerFollowUpItem(
                    x.Id,
                    x.CustomerId,
                    x.CustomerName,
                    x.Title,
                    x.AssignedTo,
                    x.DueAtUtc,
                    x.Priority,
                    x.IsOverdue,
                    DateOnly.FromDateTime(dueLocal.DateTime) == localToday);
            })
            .ToArray();

        var openOffers = offers
            .Where(x => x.Status is OfferStatus.Draft or OfferStatus.Sent)
            .ToArray();

        var activeProjects = projects
            .Where(x => x.Status is ProjectStatus.Planned
                or ProjectStatus.Active
                or ProjectStatus.OnHold)
            .ToArray();

        var openTasks = activeProjects
            .SelectMany(project => project.Tasks.Select(task => new { project, task }))
            .Where(x => x.task.Status != ProjectTaskStatus.Completed)
            .ToArray();

        var dueTasks = openTasks
            .Where(x => x.task.DueDate.HasValue
                && x.task.DueDate.Value <= today.AddDays(14))
            .OrderBy(x => x.task.DueDate)
            .ThenBy(x => x.project.ProjectNumber)
            .Select(x => new DashboardTaskItem(
                x.project.Id,
                x.project.ProjectNumber,
                x.project.Title,
                x.task.Id,
                x.task.Title,
                x.task.AssignedTo,
                x.task.DueDate,
                x.task.DueDate < today))
            .Take(12)
            .ToArray();

        var recentOffers = offers
            .OrderByDescending(x => x.OfferDate)
            .Take(6)
            .Select(x => new DashboardActivityItem(
                "Angebot",
                x.Id,
                x.OfferNumber,
                x.Title,
                x.Status.ToString(),
                x.OfferDate));

        var recentProjects = projects
            .OrderByDescending(x => x.PlannedStart)
            .Take(6)
            .Select(x => new DashboardActivityItem(
                "Projekt",
                x.Id,
                x.ProjectNumber,
                x.Title,
                x.Status.ToString(),
                x.PlannedStart));

        var communications = await customerCommunicationService.GetAllAsync(
            cancellationToken);

        var recentCommunications = communications
            .SelectMany(x => x.Items)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(8)
            .Select(x => new DashboardActivityItem(
                x.TypeText,
                x.DocumentId,
                x.DocumentNumber,
                x.Title,
                x.Status,
                DateOnly.FromDateTime(x.OccurredAtUtc.ToLocalTime().DateTime)));

        var recentItems = recentOffers
            .Concat(recentProjects)
            .Concat(recentCommunications)
            .OrderByDescending(x => x.ReferenceDate)
            .ThenBy(x => x.Number)
            .Take(10)
            .ToArray();

        return new DashboardDto(
            openOffers.Length,
            openOffers.Sum(x => x.NetTotal),
            activeProjects.Length,
            openTasks.Length,
            dueTasks.Length,
            dueTasks.Count(x => x.IsOverdue),
            openFollowUps.Count,
            crmFollowUps.Count(x => x.IsOverdue),
            openCustomerFollowUps.Length,
            openCustomerFollowUps.Count(x =>
                DateOnly.FromDateTime(x.DueAtUtc.ToLocalTime().DateTime) == localToday),
            openCustomerFollowUps.Count(x => x.IsOverdue),
            openCustomerFollowUps.Count(x =>
                x.Priority == CustomerFollowUpPriority.Urgent),
            dueTasks,
            crmFollowUps,
            customerFollowUps,
            recentItems);
    }
}
