using WerkPilot.Application.Auditing;
using WerkPilot.Application.Crm;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Identity;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Identity;

public sealed class UserAbsenceService(
    IUserAbsenceRepository repository,
    UserService userService,
    CustomerFollowUpService customerFollowUpService,
    ProjectService projectService,
    SessionContext session,
    IAuditTrail auditTrail)
{
    public async Task<IReadOnlyList<UserAbsenceDto>> GetAllAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var absences = await repository.GetAllAsync(cancellationToken);

        foreach (var trackedId in absences
                     .Where(x => x.Status != UserAbsenceStatus.Cancelled)
                     .Select(x => x.Id))
        {
            var tracked = await repository.GetAsync(trackedId, cancellationToken);
            tracked?.RefreshStatus(today);
        }

        await repository.SaveChangesAsync(cancellationToken);

        return (await repository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.UserDisplayName)
            .Select(x => Map(x, today))
            .ToArray();
    }

    public async Task<UserAbsenceDto> CreateAsync(
        CreateUserAbsenceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EndDate < request.StartDate)
            throw new ArgumentException("Ungültiger Abwesenheitszeitraum.");

        var users = await userService.GetAllAsync(cancellationToken);
        var user = users.SingleOrDefault(x => x.Id == request.UserId && x.IsActive)
            ?? throw new InvalidOperationException("Benutzer wurde nicht gefunden oder ist nicht aktiv.");

        UserDto? substitute = null;
        if (request.SubstituteUserId.HasValue)
        {
            substitute = users.SingleOrDefault(x =>
                x.Id == request.SubstituteUserId.Value && x.IsActive)
                ?? throw new InvalidOperationException("Vertretung wurde nicht gefunden oder ist nicht aktiv.");

            if (substitute.Id == user.Id)
                throw new InvalidOperationException("Benutzer und Vertretung dürfen nicht identisch sein.");
        }

        var overlapping = (await repository.GetAllAsync(cancellationToken))
            .Any(x => x.UserId == user.Id
                && x.Overlaps(request.StartDate, request.EndDate));

        if (overlapping)
            throw new InvalidOperationException(
                "Für diesen Benutzer existiert bereits eine überlappende Abwesenheit.");

        var absence = new UserAbsence(
            user.Id,
            user.DisplayName,
            request.Type,
            request.StartDate,
            request.EndDate,
            substitute?.Id,
            substitute?.DisplayName,
            request.Note,
            session.DisplayName);

        absence.RefreshStatus(DateOnly.FromDateTime(DateTime.Today));

        await repository.AddAsync(absence, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "UserAbsence",
            absence.Id,
            "Created",
            $"Abwesenheit für {user.DisplayName} von {request.StartDate:dd.MM.yyyy} bis {request.EndDate:dd.MM.yyyy} angelegt.",
            cancellationToken);

        return Map(absence, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var absence = await repository.GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Abwesenheit wurde nicht gefunden.");

        absence.Cancel();
        await repository.SaveChangesAsync(cancellationToken);

        await auditTrail.WriteAsync(
            "UserAbsence",
            absence.Id,
            "Cancelled",
            $"Abwesenheit von {absence.UserDisplayName} wurde storniert.",
            cancellationToken);
    }


    public async Task<UserAbsenceWorkPreviewDto> GetWorkPreviewAsync(
        Guid absenceId,
        CancellationToken cancellationToken = default)
    {
        var absence = await repository.GetAsync(absenceId, cancellationToken)
            ?? throw new InvalidOperationException("Abwesenheit wurde nicht gefunden.");

        var items = new List<UserAbsenceAffectedWorkItemDto>();

        var followUps = await customerFollowUpService.GetAllAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);

        foreach (var followUp in followUps.Where(x =>
                     (x.Status is CustomerFollowUpStatus.Open
                         or CustomerFollowUpStatus.InProgress)
                     && BelongsTo(
                         x.AssignedUserId,
                         x.AssignedTo,
                         absence.UserId,
                         absence.UserDisplayName)))
        {
            var dueDate = DateOnly.FromDateTime(
                followUp.DueAtUtc.ToLocalTime().DateTime);

            items.Add(new UserAbsenceAffectedWorkItemDto(
                UserAbsenceAffectedWorkType.CustomerFollowUp,
                followUp.Id,
                null,
                followUp.CustomerNumber,
                followUp.CustomerName,
                followUp.Title,
                followUp.Priority.ToString(),
                dueDate,
                IsWithin(dueDate, absence.StartDate, absence.EndDate)));
        }

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
                             absence.UserId,
                             absence.UserDisplayName)))
            {
                items.Add(new UserAbsenceAffectedWorkItemDto(
                    UserAbsenceAffectedWorkType.ProjectTask,
                    task.Id,
                    project.Id,
                    project.ProjectNumber,
                    project.Title,
                    task.Title,
                    "Normal",
                    task.DueDate,
                    task.DueDate.HasValue
                        && IsWithin(
                            task.DueDate.Value,
                            absence.StartDate,
                            absence.EndDate)));
            }
        }

        var ordered = items
            .OrderByDescending(x => x.DueDuringAbsence)
            .ThenBy(x => x.DueDate ?? DateOnly.MaxValue)
            .ThenBy(x => x.Context)
            .ToArray();

        return new UserAbsenceWorkPreviewDto(
            absence.Id,
            absence.UserDisplayName,
            absence.SubstituteDisplayName,
            ordered.Length,
            ordered.Count(x => x.DueDuringAbsence),
            ordered);
    }

    public async Task<UserAbsenceTransferResult> TransferToSubstituteAsync(
        Guid absenceId,
        bool onlyDueDuringAbsence,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "Ein Übergabegrund ist erforderlich.",
                nameof(reason));

        var absence = await repository.GetAsync(absenceId, cancellationToken)
            ?? throw new InvalidOperationException("Abwesenheit wurde nicht gefunden.");

        if (absence.Status == UserAbsenceStatus.Cancelled)
            throw new InvalidOperationException(
                "Für eine stornierte Abwesenheit können keine Aufgaben übergeben werden.");

        if (!absence.SubstituteUserId.HasValue
            || string.IsNullOrWhiteSpace(absence.SubstituteDisplayName))
        {
            throw new InvalidOperationException(
                "Für diese Abwesenheit ist keine Vertretung hinterlegt.");
        }

        var preview = await GetWorkPreviewAsync(absenceId, cancellationToken);
        var selected = onlyDueDuringAbsence
            ? preview.Items.Where(x => x.DueDuringAbsence).ToArray()
            : preview.Items.ToArray();

        var customerCount = 0;
        var projectCount = 0;

        foreach (var item in selected)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.Type == UserAbsenceAffectedWorkType.CustomerFollowUp)
            {
                await customerFollowUpService.ReassignAsync(
                    item.SourceId,
                    absence.SubstituteUserId.Value,
                    absence.SubstituteDisplayName,
                    reason,
                    cancellationToken);

                customerCount++;
                continue;
            }

            if (item.Type == UserAbsenceAffectedWorkType.ProjectTask
                && item.ProjectId.HasValue)
            {
                await projectService.ReassignTaskAsync(
                    item.ProjectId.Value,
                    item.SourceId,
                    absence.SubstituteUserId.Value,
                    absence.SubstituteDisplayName,
                    reason,
                    cancellationToken);

                projectCount++;
            }
        }

        await auditTrail.WriteAsync(
            "UserAbsence",
            absence.Id,
            "WorkTransferred",
            $"{customerCount + projectCount} Aufgabe(n) von {absence.UserDisplayName} "
            + $"an {absence.SubstituteDisplayName} übergeben. "
            + $"Umfang: {(onlyDueDuringAbsence ? "nur im Zeitraum fällige Aufgaben" : "alle offenen Aufgaben")}. "
            + $"Grund: {reason.Trim()}",
            cancellationToken);

        return new UserAbsenceTransferResult(
            absence.UserDisplayName,
            absence.SubstituteDisplayName,
            onlyDueDuringAbsence,
            customerCount,
            projectCount,
            customerCount + projectCount);
    }

    public async Task<UserAbsenceConflictDto> GetConflictAsync(
        Guid absenceId,
        CancellationToken cancellationToken = default)
    {
        var absence = await repository.GetAsync(absenceId, cancellationToken)
            ?? throw new InvalidOperationException("Abwesenheit wurde nicht gefunden.");

        var followUps = await customerFollowUpService.GetAllAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);

        var customer = followUps.Where(x =>
            (x.Status is CustomerFollowUpStatus.Open or CustomerFollowUpStatus.InProgress)
            && BelongsTo(x.AssignedUserId, x.AssignedTo, absence.UserId, absence.UserDisplayName))
            .ToArray();

        var projects = await projectService.GetAllAsync(cancellationToken);
        var projectTasks = projects
            .Where(x => x.Status is ProjectStatus.Planned or ProjectStatus.Active or ProjectStatus.OnHold)
            .SelectMany(x => x.Tasks)
            .Where(x =>
                x.Status != ProjectTaskStatus.Completed
                && BelongsTo(x.AssignedUserId, x.AssignedTo, absence.UserId, absence.UserDisplayName))
            .ToArray();

        var dueDuring = customer.Count(x =>
                DateOnly.FromDateTime(x.DueAtUtc.ToLocalTime().DateTime) >= absence.StartDate
                && DateOnly.FromDateTime(x.DueAtUtc.ToLocalTime().DateTime) <= absence.EndDate)
            + projectTasks.Count(x =>
                x.DueDate.HasValue
                && x.DueDate.Value >= absence.StartDate
                && x.DueDate.Value <= absence.EndDate);

        return new UserAbsenceConflictDto(
            absence.UserId,
            absence.UserDisplayName,
            customer.Length,
            projectTasks.Length,
            dueDuring,
            absence.SubstituteUserId.HasValue);
    }


    private static bool IsWithin(
        DateOnly date,
        DateOnly startDate,
        DateOnly endDate) =>
        date >= startDate && date <= endDate;

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

    private static UserAbsenceDto Map(UserAbsence x, DateOnly today) => new(
        x.Id,
        x.UserId,
        x.UserDisplayName,
        x.Type,
        x.StartDate,
        x.EndDate,
        x.SubstituteUserId,
        x.SubstituteDisplayName,
        x.Note,
        x.Status,
        x.Includes(today),
        x.EndDate.DayNumber - x.StartDate.DayNumber + 1);
}
