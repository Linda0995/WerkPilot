using WerkPilot.Application.Crm;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Work;

public sealed class TeamWorkService(
    UserService userService,
    CustomerFollowUpService customerFollowUpService,
    ProjectService projectService)
{
    public async Task<TeamWorkSummaryDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var users = (await userService.GetAllAsync(cancellationToken))
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayName)
            .ToArray();

        var nowUtc = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var followUps = await customerFollowUpService.GetAllAsync(
            nowUtc,
            cancellationToken);

        var projects = await projectService.GetAllAsync(cancellationToken);

        var summaries = new List<TeamWorkUserSummaryDto>(users.Length);

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = new List<MyWorkItemDto>();

            foreach (var followUp in followUps.Where(x =>
                         (x.Status is CustomerFollowUpStatus.Open
                             or CustomerFollowUpStatus.InProgress)
                         && IsAssigned(x, user)))
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
                    user.DisplayName,
                    followUp.DueAtUtc,
                    followUp.Priority.ToString(),
                    followUp.Status.ToString(),
                    DateOnly.FromDateTime(dueLocal.DateTime) == today,
                    followUp.IsOverdue));
            }

            foreach (var project in projects.Where(x =>
                         x.Status is ProjectStatus.Planned
                             or ProjectStatus.Active
                             or ProjectStatus.OnHold))
            {
                foreach (var task in project.Tasks.Where(x =>
                             x.Status != ProjectTaskStatus.Completed
                             && IsProjectTaskAssigned(x, user)))
                {
                    DateTimeOffset? dueAtUtc = task.DueDate.HasValue
                        ? new DateTimeOffset(
                            task.DueDate.Value.ToDateTime(TimeOnly.MinValue),
                            TimeZoneInfo.Local.GetUtcOffset(
                                task.DueDate.Value.ToDateTime(TimeOnly.MinValue)))
                            .ToUniversalTime()
                        : null;

                    items.Add(new MyWorkItemDto(
                        MyWorkItemType.ProjectTask,
                        task.Id,
                        project.CustomerId,
                        project.Id,
                        project.ProjectNumber,
                        project.Title,
                        task.Title,
                        user.DisplayName,
                        dueAtUtc,
                        "Normal",
                        task.Status.ToString(),
                        task.DueDate == today,
                        task.DueDate.HasValue && task.DueDate.Value < today));
                }
            }

            var ordered = items
                .OrderByDescending(x => x.IsOverdue)
                .ThenByDescending(x => x.IsDueToday)
                .ThenBy(x => PriorityRank(x.Priority))
                .ThenBy(x => x.DueAtUtc ?? DateTimeOffset.MaxValue)
                .ToArray();

            summaries.Add(new TeamWorkUserSummaryDto(
                user.Id,
                user.UserName,
                user.DisplayName,
                user.Role,
                ordered.Length,
                ordered.Count(x => x.IsDueToday),
                ordered.Count(x => x.IsOverdue),
                ordered.Count(x =>
                    string.Equals(
                        x.Priority,
                        "Urgent",
                        StringComparison.OrdinalIgnoreCase)),
                ordered.Count(x => x.Type == MyWorkItemType.CustomerFollowUp),
                ordered.Count(x => x.Type == MyWorkItemType.ProjectTask),
                ordered.Where(x => x.DueAtUtc.HasValue)
                    .OrderBy(x => x.DueAtUtc)
                    .Select(x => x.DueAtUtc)
                    .FirstOrDefault(),
                ordered));
        }

        return new TeamWorkSummaryDto(
            summaries.Count,
            summaries.Sum(x => x.OpenCount),
            summaries.Sum(x => x.DueTodayCount),
            summaries.Sum(x => x.OverdueCount),
            summaries.Sum(x => x.UrgentCount),
            summaries
                .OrderByDescending(x => x.OverdueCount)
                .ThenByDescending(x => x.UrgentCount)
                .ThenByDescending(x => x.OpenCount)
                .ThenBy(x => x.DisplayName)
                .ToArray());
    }


    private static bool IsProjectTaskAssigned(
        ProjectTaskDto task,
        UserDto user)
    {
        if (task.AssignedUserId.HasValue)
            return task.AssignedUserId.Value == user.Id;

        return string.Equals(
            task.AssignedTo?.Trim(),
            user.DisplayName,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAssigned(
        CustomerFollowUpDto followUp,
        UserDto user)
    {
        if (followUp.AssignedUserId.HasValue)
            return followUp.AssignedUserId.Value == user.Id;

        return string.Equals(
            followUp.AssignedTo?.Trim(),
            user.DisplayName,
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
