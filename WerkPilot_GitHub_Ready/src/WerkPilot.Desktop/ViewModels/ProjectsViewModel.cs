using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Desktop.ViewModels;

public sealed class ProjectsViewModel : INotifyPropertyChanged
{
    private readonly ProjectService _projects;
    private readonly OfferService _offers;
    private readonly UserService _users;
    private ProjectDto? _selectedProject;
    private ProjectTaskDto? _selectedTask;
    private OfferDto? _selectedOffer;
    private string _projectTitle = string.Empty;
    private string? _projectDescription;
    private string? _projectManager;
    private DateTimeOffset? _plannedStart = DateTimeOffset.Now;
    private DateTimeOffset? _plannedEnd;
    private ProjectStatus _projectStatus = ProjectStatus.Planned;
    private string _taskTitle = string.Empty;
    private UserAssignmentOption? _selectedTaskAssignee;
    private string? _taskAssignedTo;
    private DateTimeOffset? _taskDueDate;
    private ProjectTaskStatus _taskStatus = ProjectTaskStatus.Open;
    private string _statusText = "Bereit";

    public ProjectsViewModel(
        ProjectService projects,
        OfferService offers,
        UserService users)
    {
        _projects = projects;
        _offers = offers;
        _users = users;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateFromOfferCommand = new AsyncCommand(CreateFromOfferAsync, CanCreateFromOffer);
        SaveProjectCommand = new AsyncCommand(SaveProjectAsync, HasSelectedProject);
        ApplyProjectStatusCommand = new AsyncCommand(ApplyProjectStatusAsync, HasSelectedProject);
        AddTaskCommand = new AsyncCommand(AddTaskAsync, HasSelectedProject);
        UpdateTaskCommand = new AsyncCommand(UpdateTaskAsync, HasSelectedTask);
        RemoveTaskCommand = new AsyncCommand(RemoveTaskAsync, HasSelectedTask);

        _ = InitializeAsync();
    }

    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<ProjectTaskDto> Tasks { get; } = [];
    public ObservableCollection<OfferDto> AcceptedOffers { get; } = [];
    public ObservableCollection<UserAssignmentOption> TaskAssignees { get; } = [];
    public IReadOnlyList<ProjectStatus> ProjectStatuses { get; } =
        Enum.GetValues<ProjectStatus>();
    public IReadOnlyList<ProjectTaskStatus> TaskStatuses { get; } =
        Enum.GetValues<ProjectTaskStatus>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateFromOfferCommand { get; }
    public ICommand SaveProjectCommand { get; }
    public ICommand ApplyProjectStatusCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand UpdateTaskCommand { get; }
    public ICommand RemoveTaskCommand { get; }

    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (Set(ref _selectedProject, value))
            {
                LoadProject(value);
                RefreshCommands();
            }
        }
    }

    public ProjectTaskDto? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (Set(ref _selectedTask, value))
            {
                LoadTask(value);
                RefreshCommands();
            }
        }
    }

    public OfferDto? SelectedOffer
    {
        get => _selectedOffer;
        set
        {
            Set(ref _selectedOffer, value);
            RefreshCommands();
        }
    }

    public string ProjectTitle { get => _projectTitle; set => Set(ref _projectTitle, value); }
    public string? ProjectDescription { get => _projectDescription; set => Set(ref _projectDescription, value); }
    public string? ProjectManager { get => _projectManager; set => Set(ref _projectManager, value); }
    public DateTimeOffset? PlannedStart { get => _plannedStart; set => Set(ref _plannedStart, value); }
    public DateTimeOffset? PlannedEnd { get => _plannedEnd; set => Set(ref _plannedEnd, value); }
    public ProjectStatus ProjectStatus { get => _projectStatus; set => Set(ref _projectStatus, value); }
    public string TaskTitle { get => _taskTitle; set => Set(ref _taskTitle, value); }
    public UserAssignmentOption? SelectedTaskAssignee
    {
        get => _selectedTaskAssignee;
        set
        {
            if (Set(ref _selectedTaskAssignee, value))
                TaskAssignedTo = value?.UserId is null ? null : value.DisplayName;
        }
    }
    public string? TaskAssignedTo { get => _taskAssignedTo; set => Set(ref _taskAssignedTo, value); }
    public DateTimeOffset? TaskDueDate { get => _taskDueDate; set => Set(ref _taskDueDate, value); }
    public ProjectTaskStatus TaskStatus { get => _taskStatus; set => Set(ref _taskStatus, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        var offers = await _offers.GetAllAsync();
        AcceptedOffers.Clear();

        foreach (var offer in offers.Where(x => x.Status == OfferStatus.Accepted))
            AcceptedOffers.Add(offer);

        TaskAssignees.Add(UserAssignmentOption.Unassigned);

        foreach (var user in (await _users.GetAllAsync())
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.DisplayName))
        {
            TaskAssignees.Add(new UserAssignmentOption(user.Id, user.DisplayName));
        }

        SelectedTaskAssignee = TaskAssignees.FirstOrDefault();
        SelectedOffer = AcceptedOffers.FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedProject?.Id;
            var projects = await _projects.GetAllAsync();

            Projects.Clear();
            foreach (var project in projects)
                Projects.Add(project);

            SelectedProject = selectedId is null
                ? Projects.FirstOrDefault()
                : Projects.FirstOrDefault(x => x.Id == selectedId);

            StatusText = $"{Projects.Count} Projekt(e) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projekte konnten nicht geladen werden");
        }
    }

    private async Task CreateFromOfferAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var project = await _projects.CreateFromAcceptedOfferAsync(SelectedOffer.Id);
            await RefreshAsync();
            SelectedProject = Projects.FirstOrDefault(x => x.Id == project.Id);
            StatusText = $"Projekt {project.ProjectNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projekt konnte nicht angelegt werden");
        }
    }

    private async Task SaveProjectAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            await _projects.UpdateAsync(new UpdateProjectRequest(
                SelectedProject.Id,
                ProjectTitle,
                ProjectDescription,
                ProjectManager,
                DateOnly.FromDateTime((PlannedStart ?? DateTimeOffset.Now).DateTime),
                PlannedEnd.HasValue
                    ? DateOnly.FromDateTime(PlannedEnd.Value.DateTime)
                    : null));

            await RefreshAsync();
            StatusText = "Projektdaten wurden gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projekt konnte nicht gespeichert werden");
        }
    }

    private async Task ApplyProjectStatusAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            await _projects.SetStatusAsync(SelectedProject.Id, ProjectStatus);
            await RefreshAsync();
            StatusText = "Projektstatus wurde geändert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projektstatus konnte nicht geändert werden");
        }
    }

    private async Task AddTaskAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            await _projects.AddTaskAsync(
                SelectedProject.Id,
                TaskTitle,
                SelectedTaskAssignee?.UserId,
                TaskAssignedTo,
                TaskDueDate.HasValue
                    ? DateOnly.FromDateTime(TaskDueDate.Value.DateTime)
                    : null);

            ClearTaskEditor();
            await RefreshAsync();
            StatusText = "Projektaufgabe wurde hinzugefügt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Aufgabe konnte nicht hinzugefügt werden");
        }
    }

    private async Task UpdateTaskAsync()
    {
        if (SelectedProject is null || SelectedTask is null)
            return;

        try
        {
            await _projects.UpdateTaskAsync(new UpdateProjectTaskRequest(
                SelectedProject.Id,
                SelectedTask.Id,
                TaskTitle,
                SelectedTaskAssignee?.UserId,
                TaskAssignedTo,
                TaskDueDate.HasValue
                    ? DateOnly.FromDateTime(TaskDueDate.Value.DateTime)
                    : null,
                TaskStatus));

            await RefreshAsync();
            StatusText = "Projektaufgabe wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Aufgabe konnte nicht aktualisiert werden");
        }
    }

    private async Task RemoveTaskAsync()
    {
        if (SelectedProject is null || SelectedTask is null)
            return;

        try
        {
            await _projects.RemoveTaskAsync(SelectedProject.Id, SelectedTask.Id);
            ClearTaskEditor();
            await RefreshAsync();
            StatusText = "Projektaufgabe wurde entfernt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Aufgabe konnte nicht entfernt werden");
        }
    }

    private void LoadProject(ProjectDto? project)
    {
        ProjectTitle = project?.Title ?? string.Empty;
        ProjectDescription = project?.Description;
        ProjectManager = project?.ProjectManager;
        PlannedStart = project is null
            ? DateTimeOffset.Now
            : new DateTimeOffset(project.PlannedStart.ToDateTime(TimeOnly.MinValue));
        PlannedEnd = project?.PlannedEnd is { } end
            ? new DateTimeOffset(end.ToDateTime(TimeOnly.MinValue))
            : null;
        ProjectStatus = project?.Status ?? ProjectStatus.Planned;

        Tasks.Clear();
        if (project is not null)
            foreach (var task in project.Tasks)
                Tasks.Add(task);

        SelectedTask = null;
    }

    private void LoadTask(ProjectTaskDto? task)
    {
        TaskTitle = task?.Title ?? string.Empty;
        TaskAssignedTo = task?.AssignedTo;
        SelectedTaskAssignee = task?.AssignedUserId is { } userId
            ? TaskAssignees.FirstOrDefault(x => x.UserId == userId)
            : TaskAssignees.FirstOrDefault(x =>
                string.Equals(
                    x.DisplayName,
                    task?.AssignedTo,
                    StringComparison.OrdinalIgnoreCase))
              ?? UserAssignmentOption.Unassigned;
        TaskDueDate = task?.DueDate is { } dueDate
            ? new DateTimeOffset(dueDate.ToDateTime(TimeOnly.MinValue))
            : null;
        TaskStatus = task?.Status ?? ProjectTaskStatus.Open;
    }

    private void ClearTaskEditor()
    {
        SelectedTask = null;
        TaskTitle = string.Empty;
        SelectedTaskAssignee = TaskAssignees.FirstOrDefault();
        TaskAssignedTo = null;
        TaskDueDate = null;
        TaskStatus = ProjectTaskStatus.Open;
    }

    private bool CanCreateFromOffer() => SelectedOffer is not null;
    private bool HasSelectedProject() => SelectedProject is not null;
    private bool HasSelectedTask() =>
        SelectedProject is not null && SelectedTask is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateFromOfferCommand,
            SaveProjectCommand,
            ApplyProjectStatusCommand,
            AddTaskCommand,
            UpdateTaskCommand,
            RemoveTaskCommand
        })
            (command as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }

    private sealed class AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null) : ICommand
    {
        private bool _running;

        public bool CanExecute(object? parameter) =>
            !_running && (canExecute?.Invoke() ?? true);

        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _running = true;
                RaiseCanExecuteChanged();
                await execute();
            }
            finally
            {
                _running = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
