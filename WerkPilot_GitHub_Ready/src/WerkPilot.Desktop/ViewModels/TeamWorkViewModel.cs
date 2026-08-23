using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Work;

namespace WerkPilot.Desktop.ViewModels;

public sealed class TeamWorkViewModel : INotifyPropertyChanged
{
    private readonly TeamWorkService _service;
    private TeamWorkUserSummaryDto? _selectedUser;
    private int _activeUserCount;
    private int _openCount;
    private int _dueTodayCount;
    private int _overdueCount;
    private int _urgentCount;
    private string _statusText = "Bereit";

    public TeamWorkViewModel(TeamWorkService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<TeamWorkUserSummaryDto> Users { get; } = [];
    public ObservableCollection<MyWorkItemDto> Items { get; } = [];
    public ICommand RefreshCommand { get; }

    public TeamWorkUserSummaryDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (Set(ref _selectedUser, value))
            {
                Items.Clear();
                if (value is not null)
                    foreach (var item in value.Items)
                        Items.Add(item);
            }
        }
    }

    public int ActiveUserCount { get => _activeUserCount; private set => Set(ref _activeUserCount, value); }
    public int OpenCount { get => _openCount; private set => Set(ref _openCount, value); }
    public int DueTodayCount { get => _dueTodayCount; private set => Set(ref _dueTodayCount, value); }
    public int OverdueCount { get => _overdueCount; private set => Set(ref _overdueCount, value); }
    public int UrgentCount { get => _urgentCount; private set => Set(ref _urgentCount, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedUser?.UserId;
            var summary = await _service.GetAsync();

            ActiveUserCount = summary.ActiveUserCount;
            OpenCount = summary.OpenCount;
            DueTodayCount = summary.DueTodayCount;
            OverdueCount = summary.OverdueCount;
            UrgentCount = summary.UrgentCount;

            Users.Clear();
            foreach (var user in summary.Users)
                Users.Add(user);

            SelectedUser = selectedId.HasValue
                ? Users.FirstOrDefault(x => x.UserId == selectedId.Value)
                : Users.FirstOrDefault();

            StatusText = $"{Users.Count} aktive Benutzer geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Team-Arbeit konnte nicht geladen werden");
        }
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

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running;
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (_running) return;
            try
            {
                _running = true;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await execute();
            }
            finally
            {
                _running = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
