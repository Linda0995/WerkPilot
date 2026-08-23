using WerkPilot.Application.Crm;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Work;

public sealed class WorkReassignmentService(
    UserService userService,
    CustomerFollowUpService customerFollowUpService,
    ProjectService projectService)
{
    public async Task<ReassignWorkResult> ReassignOpenWorkAsync(
        ReassignWorkRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceUserId == request.TargetUserId)
            throw new InvalidOperationException(
                "Quelle und Ziel der Übergabe dürfen nicht identisch sein.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException(
                "Für eine Aufgabenübergabe ist ein Grund erforderlich.",
                nameof(request));

        if (!request.IncludeCustomerFollowUps && !request.IncludeProjectTasks)
            throw new InvalidOperationException(
                "Mindestens ein Aufgabenbereich muss ausgewählt sein.");

        var users = await userService.GetAllAsync(cancellationToken);

        var source = users.SingleOrDefault(x =>
            x.Id == request.SourceUserId && x.IsActive)
            ?? throw new InvalidOperationException(
                "Der bisher verantwortliche Benutzer wurde nicht gefunden oder ist nicht aktiv.");

        var target = users.SingleOrDefault(x =>
            x.Id == request.TargetUserId && x.IsActive)
            ?? throw new InvalidOperationException(
                "Der neue verantwortliche Benutzer wurde nicht gefunden oder ist nicht aktiv.");

        var followUpCount = 0;
        var projectTaskCount = 0;

        if (request.IncludeCustomerFollowUps)
        {
            var followUps = await customerFollowUpService.GetAllAsync(
                DateTimeOffset.UtcNow,
                cancellationToken);

            foreach (var followUp in followUps.Where(x =>
                         (x.Status is CustomerFollowUpStatus.Open
                             or CustomerFollowUpStatus.InProgress)
                         && BelongsTo(
                             x.AssignedUserId,
                             x.AssignedTo,
                             source.Id,
                             source.DisplayName)))
            {
                await customerFollowUpService.ReassignAsync(
                    followUp.Id,
                    target.Id,
                    target.DisplayName,
                    request.Reason,
                    cancellationToken);

                followUpCount++;
            }
        }

        if (request.IncludeProjectTasks)
        {
            var projects = await projectService.GetAllAsync(cancellationToken);

            foreach (var project in projects.Where(x =>
                         x.Status is ProjectStatus.Planned
                             or ProjectStatus.Active
                             or ProjectStatus.OnHold))
            {
                foreach (var task in project.Tasks.Where(x =>
                             x.Status != ProjectTaskStatus.Completed
                             && BelongsTo(
                                 x.AssignedUserId,
                                 x.AssignedTo,
                                 source.Id,
                                 source.DisplayName)))
                {
                    await projectService.ReassignTaskAsync(
                        project.Id,
                        task.Id,
                        target.Id,
                        target.DisplayName,
                        request.Reason,
                        cancellationToken);

                    projectTaskCount++;
                }
            }
        }

        return new ReassignWorkResult(
            source.DisplayName,
            target.DisplayName,
            followUpCount,
            projectTaskCount,
            followUpCount + projectTaskCount);
    }

    private static bool BelongsTo(
        Guid? assignedUserId,
        string? assignedTo,
        Guid userId,
        string displayName)
    {
        if (assignedUserId.HasValue)
            return assignedUserId.Value == userId;

        return string.Equals(
            assignedTo?.Trim(),
            displayName,
            StringComparison.OrdinalIgnoreCase);
    }
}
