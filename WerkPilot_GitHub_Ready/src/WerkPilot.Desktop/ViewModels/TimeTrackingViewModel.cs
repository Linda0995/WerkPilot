using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Projects;
using WerkPilot.Application.TimeTracking;

namespace WerkPilot.Desktop.ViewModels;

public sealed class TimeTrackingViewModel : INotifyPropertyChanged
{
    private readonly TimeTrackingService _timeTracking;
    private readonly ProjectService _projects;
    private readonly ProjectTimeControllingService _controlling;
    private ProjectDto? _selectedProject;
    private ProjectTaskDto? _selectedTask;
    private TimeEntryDto? _selectedEntry;
    private string _description = string.Empty;
    private DateTimeOffset? _manualStart = DateTimeOffset.Now;
    private DateTimeOffset? _manualEnd = DateTimeOffset.Now.AddHours(1);
    private string _runningText = "Keine laufende Zeiterfassung";
    private decimal _totalHours;
    private decimal _completedHours;
    private decimal _plannedLaborHours;
    private decimal _remainingHours;
    private decimal _varianceHours;
    private decimal _utilizationPercent;
    private decimal _plannedLaborCost;
    private decimal _actualLaborCostEstimate;
    private string _controllingStatusText = "Kein Zeitbudget";
    private string _statusText = "Bereit";

    public TimeTrackingViewModel(
        TimeTrackingService timeTracking,
        ProjectService projects,
        ProjectTimeControllingService controlling)
    {
        _timeTracking = timeTracking;
        _projects = projects;
        _controlling = controlling;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        StartCommand = new AsyncCommand(StartAsync, CanStart);
        StopCommand = new AsyncCommand(StopAsync);
        AddManualCommand = new AsyncCommand(AddManualAsync, CanAddManual);
        UpdateManualCommand = new AsyncCommand(UpdateManualAsync, HasSelectedEntry);

        _ = InitializeAsync();
    }

    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<ProjectTaskDto> Tasks { get; } = [];
    public ObservableCollection<TimeEntryDto> Entries { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand AddManualCommand { get; }
    public ICommand UpdateManualCommand { get; }

    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (Set(ref _selectedProject, value))
            {
                Tasks.Clear();
                if (value is not null)
                    foreach (var task in value.Tasks)
                        Tasks.Add(task);

                SelectedTask = null;
                _ = RefreshAsync();
                RefreshCommands();
            }
        }
    }

    public ProjectTaskDto? SelectedTask
    {
        get => _selectedTask;
        set => Set(ref _selectedTask, value);
    }

    public TimeEntryDto? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (Set(ref _selectedEntry, value) && value is not null)
            {
                Description = value.Description;
                ManualStart = value.StartedAtUtc.ToLocalTime();
                ManualEnd = value.EndedAtUtc?.ToLocalTime();
                SelectedTask = Tasks.FirstOrDefault(x => x.Id == value.ProjectTaskId);
            }

            RefreshCommands();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            Set(ref _description, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? ManualStart
    {
        get => _manualStart;
        set
        {
            Set(ref _manualStart, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? ManualEnd
    {
        get => _manualEnd;
        set
        {
            Set(ref _manualEnd, value);
            RefreshCommands();
        }
    }

    public string RunningText { get => _runningText; private set => Set(ref _runningText, value); }
    public decimal TotalHours { get => _totalHours; private set => Set(ref _totalHours, value); }
    public decimal CompletedHours { get => _completedHours; private set => Set(ref _completedHours, value); }
    public decimal PlannedLaborHours { get => _plannedLaborHours; private set => Set(ref _plannedLaborHours, value); }
    public decimal RemainingHours { get => _remainingHours; private set => Set(ref _remainingHours, value); }
    public decimal VarianceHours { get => _varianceHours; private set => Set(ref _varianceHours, value); }
    public decimal UtilizationPercent { get => _utilizationPercent; private set => Set(ref _utilizationPercent, value); }
    public decimal PlannedLaborCost { get => _plannedLaborCost; private set => Set(ref _plannedLaborCost, value); }
    public decimal ActualLaborCostEstimate { get => _actualLaborCostEstimate; private set => Set(ref _actualLaborCostEstimate, value); }
    public string ControllingStatusText { get => _controllingStatusText; private set => Set(ref _controllingStatusText, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var project in await _projects.GetAllAsync())
            Projects.Add(project);

        SelectedProject = Projects.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var running = await _timeTracking.GetRunningAsync();
            RunningText = running is null
                ? "Keine laufende Zeiterfassung"
                : $"{running.Description} · seit {running.StartedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm}";

            Entries.Clear();

            if (SelectedProject is not null)
            {
                foreach (var entry in await _timeTracking.GetForProjectAsync(SelectedProject.Id))
                    Entries.Add(entry);

                var summary = await _timeTracking.GetProjectSummaryAsync(SelectedProject.Id);
                TotalHours = summary.TotalHours;
                CompletedHours = summary.CompletedHours;

                var controlling = await _controlling.GetAsync(SelectedProject.Id);
                PlannedLaborHours = controlling.PlannedLaborHours;
                RemainingHours = controlling.RemainingHours;
                VarianceHours = controlling.VarianceHours;
                UtilizationPercent = controlling.UtilizationPercent;
                PlannedLaborCost = controlling.PlannedLaborCost;
                ActualLaborCostEstimate = controlling.ActualLaborCostEstimate;
                ControllingStatusText = controlling.Status switch
                {
                    ProjectTimeControllingStatus.NoBudget => "Kein Arbeitszeitbudget vorhanden",
                    ProjectTimeControllingStatus.OnTrack => "Im Zeitbudget",
                    ProjectTimeControllingStatus.Warning => "Zeitbudget zu mindestens 85 % verbraucht",
                    ProjectTimeControllingStatus.Exceeded => "Zeitbudget überschritten",
                    _ => controlling.Status.ToString()
                };
            }
            else
            {
                TotalHours = 0m;
                CompletedHours = 0m;
                PlannedLaborHours = 0m;
                RemainingHours = 0m;
                VarianceHours = 0m;
                UtilizationPercent = 0m;
                PlannedLaborCost = 0m;
                ActualLaborCostEstimate = 0m;
                ControllingStatusText = "Kein Projekt ausgewählt";
            }

            StatusText = $"{Entries.Count} Zeiteintrag/Zeiteinträge geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zeiterfassung konnte nicht geladen werden");
        }
    }

    private async Task StartAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            await _timeTracking.StartAsync(
                SelectedProject.Id,
                SelectedTask?.Id,
                Description);

            await RefreshAsync();
            StatusText = "Zeiterfassung wurde gestartet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zeiterfassung konnte nicht gestartet werden");
        }
    }

    private async Task StopAsync()
    {
        try
        {
            await _timeTracking.StopAsync();
            await RefreshAsync();
            StatusText = "Zeiterfassung wurde beendet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zeiterfassung konnte nicht beendet werden");
        }
    }

    private async Task AddManualAsync()
    {
        if (SelectedProject is null || !ManualStart.HasValue || !ManualEnd.HasValue)
            return;

        try
        {
            await _timeTracking.AddManualAsync(
                SelectedProject.Id,
                SelectedTask?.Id,
                Description,
                ManualStart.Value.ToUniversalTime(),
                ManualEnd.Value.ToUniversalTime());

            await RefreshAsync();
            StatusText = "Manueller Zeiteintrag wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zeiteintrag konnte nicht gespeichert werden");
        }
    }

    private async Task UpdateManualAsync()
    {
        if (SelectedEntry is null || !ManualStart.HasValue || !ManualEnd.HasValue)
            return;

        try
        {
            await _timeTracking.UpdateManualAsync(
                SelectedEntry.Id,
                Description,
                ManualStart.Value.ToUniversalTime(),
                ManualEnd.Value.ToUniversalTime(),
                SelectedTask?.Id);

            await RefreshAsync();
            StatusText = "Zeiteintrag wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zeiteintrag konnte nicht aktualisiert werden");
        }
    }

    private bool CanStart() =>
        SelectedProject is not null &&
        !string.IsNullOrWhiteSpace(Description);

    private bool CanAddManual() =>
        CanStart() &&
        ManualStart.HasValue &&
        ManualEnd.HasValue;

    private bool HasSelectedEntry() => SelectedEntry is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[] { StartCommand, AddManualCommand, UpdateManualCommand })
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
