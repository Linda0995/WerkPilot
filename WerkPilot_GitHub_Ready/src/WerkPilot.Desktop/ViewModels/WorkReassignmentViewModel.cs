using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Work;

namespace WerkPilot.Desktop.ViewModels;

public sealed class WorkReassignmentViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;
    private readonly WorkReassignmentService _service;
    private UserDto? _sourceUser;
    private UserDto? _targetUser;
    private string _reason = string.Empty;
    private bool _includeCustomerFollowUps = true;
    private bool _includeProjectTasks = true;
    private string _statusText = "Bereit";

    public WorkReassignmentViewModel(
        UserService userService,
        WorkReassignmentService service)
    {
        _userService = userService;
        _service = service;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        TransferCommand = new AsyncCommand(TransferAsync, CanTransfer);

        _ = RefreshAsync();
    }

    public ObservableCollection<UserDto> Users { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand TransferCommand { get; }

    public UserDto? SourceUser
    {
        get => _sourceUser;
        set
        {
            Set(ref _sourceUser, value);
            RefreshCommands();
        }
    }

    public UserDto? TargetUser
    {
        get => _targetUser;
        set
        {
            Set(ref _targetUser, value);
            RefreshCommands();
        }
    }

    public string Reason
    {
        get => _reason;
        set
        {
            Set(ref _reason, value);
            RefreshCommands();
        }
    }

    public bool IncludeCustomerFollowUps
    {
        get => _includeCustomerFollowUps;
        set
        {
            Set(ref _includeCustomerFollowUps, value);
            RefreshCommands();
        }
    }

    public bool IncludeProjectTasks
    {
        get => _includeProjectTasks;
        set
        {
            Set(ref _includeProjectTasks, value);
            RefreshCommands();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task RefreshAsync()
    {
        try
        {
            var sourceId = SourceUser?.Id;
            var targetId = TargetUser?.Id;

            Users.Clear();

            foreach (var user in (await _userService.GetAllAsync())
                         .Where(x => x.IsActive)
                         .OrderBy(x => x.DisplayName))
            {
                Users.Add(user);
            }

            SourceUser = sourceId.HasValue
                ? Users.FirstOrDefault(x => x.Id == sourceId.Value)
                : Users.FirstOrDefault();

            TargetUser = targetId.HasValue
                ? Users.FirstOrDefault(x => x.Id == targetId.Value)
                : Users.Skip(1).FirstOrDefault();

            StatusText = $"{Users.Count} aktive Benutzer geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Benutzer konnten nicht geladen werden");
        }
    }

    private async Task TransferAsync()
    {
        if (SourceUser is null || TargetUser is null)
            return;

        try
        {
            var result = await _service.ReassignOpenWorkAsync(
                new ReassignWorkRequest(
                    SourceUser.Id,
                    TargetUser.Id,
                    Reason,
                    IncludeCustomerFollowUps,
                    IncludeProjectTasks));

            StatusText =
                $"{result.TotalTransferred} Aufgabe(n) von {result.SourceUserName} an "
                + $"{result.TargetUserName} übergeben "
                + $"({result.CustomerFollowUpsTransferred} Kunden, "
                + $"{result.ProjectTasksTransferred} Projekte).";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Übergabe fehlgeschlagen");
        }
    }

    private bool CanTransfer() =>
        SourceUser is not null
        && TargetUser is not null
        && SourceUser.Id != TargetUser.Id
        && !string.IsNullOrWhiteSpace(Reason)
        && (IncludeCustomerFollowUps || IncludeProjectTasks);

    private void RefreshCommands() =>
        (TransferCommand as AsyncCommand)?.RaiseCanExecuteChanged();

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
