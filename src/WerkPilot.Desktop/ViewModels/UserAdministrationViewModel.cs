using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Identity;

namespace WerkPilot.Desktop.ViewModels;

public sealed class UserAdministrationViewModel : INotifyPropertyChanged
{
    private readonly UserService _service;
    private UserDto? _selected;
    private string _userName = string.Empty;
    private string _displayName = string.Empty;
    private string _initialPassword = string.Empty;
    private UserRole _role = UserRole.ReadOnly;
    private string _status = "Bereit";

    public UserAdministrationViewModel(UserService service)
    {
        _service = service;
        LoadCommand = new AsyncCommand(LoadAsync);
        AddCommand = new AsyncCommand(AddAsync, CanAdd);
        ToggleActiveCommand = new AsyncCommand(
            ToggleAsync,
            () => SelectedUser is not null);

        _ = LoadAsync();
    }

    public ObservableCollection<UserDto> Users { get; } = [];
    public IReadOnlyList<UserRole> Roles { get; } =
        Enum.GetValues<UserRole>();

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ToggleActiveCommand { get; }

    public UserDto? SelectedUser
    {
        get => _selected;
        set
        {
            Set(ref _selected, value);
            RefreshCommands();
        }
    }

    public string NewUserName
    {
        get => _userName;
        set
        {
            Set(ref _userName, value);
            RefreshCommands();
        }
    }

    public string NewDisplayName
    {
        get => _displayName;
        set
        {
            Set(ref _displayName, value);
            RefreshCommands();
        }
    }

    public string InitialPassword
    {
        get => _initialPassword;
        set
        {
            Set(ref _initialPassword, value);
            RefreshCommands();
        }
    }

    public UserRole NewRole
    {
        get => _role;
        set => Set(ref _role, value);
    }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    private async Task LoadAsync()
    {
        try
        {
            var users = await _service.GetAllAsync();
            Users.Clear();

            foreach (var user in users)
                Users.Add(user);

            Status = $"{Users.Count} Benutzer geladen";
        }
        catch (Exception ex)
        {
            Status = UiErrorFormatter.Format(ex, "Fehler");
        }
    }

    private async Task AddAsync()
    {
        try
        {
            await _service.CreateAsync(
                new CreateUserRequest(
                    NewUserName,
                    NewDisplayName,
                    NewRole),
                InitialPassword);

            NewUserName = string.Empty;
            NewDisplayName = string.Empty;
            InitialPassword = string.Empty;
            NewRole = UserRole.ReadOnly;

            await LoadAsync();
            Status =
                "Benutzer wurde angelegt. Kennwortwechsel beim ersten Login ist erforderlich.";
        }
        catch (Exception ex)
        {
            Status = UiErrorFormatter.Format(ex, "Vorgang fehlgeschlagen");
        }
    }

    private async Task ToggleAsync()
    {
        if (SelectedUser is null)
            return;

        await _service.SetActiveAsync(
            SelectedUser.Id,
            !SelectedUser.IsActive);

        await LoadAsync();
    }

    private bool CanAdd() =>
        !string.IsNullOrWhiteSpace(NewUserName)
        && !string.IsNullOrWhiteSpace(NewDisplayName)
        && !string.IsNullOrWhiteSpace(InitialPassword);

    private void RefreshCommands()
    {
        (AddCommand as AsyncCommand)?.Raise();
        (ToggleActiveCommand as AsyncCommand)?.Raise();
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
        Func<Task> action,
        Func<bool>? can = null) : ICommand
    {
        private bool _running;

        public bool CanExecute(object? parameter) =>
            !_running && (can?.Invoke() ?? true);

        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _running = true;
                Raise();
                await action();
            }
            finally
            {
                _running = false;
                Raise();
            }
        }

        public void Raise() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
