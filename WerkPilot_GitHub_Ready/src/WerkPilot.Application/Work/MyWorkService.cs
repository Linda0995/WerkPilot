using WerkPilot.Application.Crm;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Work;

public sealed class MyWorkService(
    SessionContext session,
    CustomerFollowUpService customerFollowUpService,
    ProjectService projectService)
{
    public async Task<MyWorkSummaryDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        if (!session.IsAuthenticated || string.IsNullOrWhiteSpace(session.DisplayName))
            throw new InvalidOperationException("Für „Meine Arbeit“ ist eine Anmeldung erforderlich.");

        var displayName = session.DisplayName.Trim();
        var userId = session.UserId;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var items = new List<MyWorkItemDto>();

        var followUps = await customerFollowUpService.GetAllAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);

        foreach (var followUp in followUps.Where(x =>
                     (x.Status is CustomerFollowUpStatus.Open
                         or CustomerFollowUpStatus.InProgress)
                     && IsAssignedToCurrentUser(
                         x.AssignedUserId,
                         x.AssignedTo,
                         userId,
                         displayName)))
        {
            var dueLocal = followUp.DueAtUtc.ToLocalTime();

            items.Add(new MyWorkItemDto(
                MyWorkItemType.CustomerFollowUp,
                followUp.Id,
                followUp.CustomerId,
                null,
                followUp.CustomerNumber,
                followUp.CustomerName,
                followUp.Title,
                followUp.AssignedTo,
                followUp.DueAtUtc,
                followUp.Priority.ToString(),
                followUp.Status.ToString(),
                DateOnly.FromDateTime(dueLocal.DateTime) == today,
                followUp.IsOverdue));
        }

        var projects = await projectService.GetAllAsync(cancellationToken);

        foreach (var project in projects.Where(x =>
                     x.Status is ProjectStatus.Planned
                         or ProjectStatus.Active
                         or ProjectStatus.OnHold))
        {
            foreach (var task in project.Tasks.Where(x =>
                         x.Status != ProjectTaskStatus.Completed
                         && IsProjectTaskAssignedToCurrentUser(
                             x.AssignedUserId,
                             x.AssignedTo,
                             userId,
                             displayName)))
            {
                DateTimeOffset? dueAtUtc = task.DueDate.HasValue
                    ? new DateTimeOffset(
                        task.DueDate.Value.ToDateTime(TimeOnly.MinValue),
                        TimeZoneInfo.Local.GetUtcOffset(
                            task.DueDate.Value.ToDateTime(TimeOnly.MinValue)))
                        .ToUniversalTime()
                    : null;

                var isDueToday = task.DueDate == today;
                var isOverdue = task.DueDate.HasValue
                    && task.DueDate.Value < today;

                items.Add(new MyWorkItemDto(
                    MyWorkItemType.ProjectTask,
                    task.Id,
                    project.CustomerId,
                    project.Id,
                    project.ProjectNumber,
                    project.Title,
                    task.Title,
                    task.AssignedTo,
                    dueAtUtc,
                    "Normal",
                    task.Status.ToString(),
                    isDueToday,
                    isOverdue));
            }
        }

        var ordered = items
            .OrderByDescending(x => x.IsOverdue)
            .ThenByDescending(x => x.IsDueToday)
            .ThenBy(x => PriorityRank(x.Priority))
            .ThenBy(x => x.DueAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.Context)
            .ToArray();

        return new MyWorkSummaryDto(
            displayName,
            ordered.Length,
            ordered.Count(x => x.IsDueToday),
            ordered.Count(x => x.IsOverdue),
            ordered.Count(x =>
                string.Equals(x.Priority, "Urgent", StringComparison.OrdinalIgnoreCase)),
            ordered.Count(x => x.Type == MyWorkItemType.CustomerFollowUp),
            ordered.Count(x => x.Type == MyWorkItemType.ProjectTask),
            ordered);
    }

    private static bool IsAssignedToCurrentUser(
        Guid? assignedUserId,
        string? assignedTo,
        Guid? currentUserId,
        string displayName)
    {
        if (assignedUserId.HasValue && currentUserId.HasValue)
            return assignedUserId.Value == currentUserId.Value;

        return string.Equals(
            assignedTo?.Trim(),
            displayName,
            StringComparison.OrdinalIgnoreCase);
    }


    private static bool IsProjectTaskAssignedToCurrentUser(
        Guid? assignedUserId,
        string? assignedTo,
        Guid? currentUserId,
        string displayName)
    {
        if (assignedUserId.HasValue && currentUserId.HasValue)
            return assignedUserId.Value == currentUserId.Value;

        return string.Equals(
            assignedTo?.Trim(),
            displayName,
            StringComparison.OrdinalIgnoreCase);
    }

    private static int PriorityRank(string priority) =>
        priority.ToLowerInvariant() switch
        {
            "urgent" => 0,
            "high" => 1,
            "normal" => 2,
            "low" => 3,
            _ => 4
        };
}
