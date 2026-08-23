using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Work;

namespace WerkPilot.Desktop.ViewModels;

public sealed class MyWorkViewModel : INotifyPropertyChanged
{
    private readonly MyWorkService _service;
    private string _userDisplayName = string.Empty;
    private int _openCount;
    private int _dueTodayCount;
    private int _overdueCount;
    private int _urgentCount;
    private int _customerFollowUpCount;
    private int _projectTaskCount;
    private bool _onlyCritical;
    private string _statusText = "Bereit";

    public MyWorkViewModel(MyWorkService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<MyWorkItemDto> Items { get; } = [];
    public ICommand RefreshCommand { get; }

    public string UserDisplayName
    {
        get => _userDisplayName;
        private set => Set(ref _userDisplayName, value);
    }

    public int OpenCount
    {
        get => _openCount;
        private set => Set(ref _openCount, value);
    }

    public int DueTodayCount
    {
        get => _dueTodayCount;
        private set => Set(ref _dueTodayCount, value);
    }

    public int OverdueCount
    {
        get => _overdueCount;
        private set => Set(ref _overdueCount, value);
    }

    public int UrgentCount
    {
        get => _urgentCount;
        private set => Set(ref _urgentCount, value);
    }

    public int CustomerFollowUpCount
    {
        get => _customerFollowUpCount;
        private set => Set(ref _customerFollowUpCount, value);
    }

    public int ProjectTaskCount
    {
        get => _projectTaskCount;
        private set => Set(ref _projectTaskCount, value);
    }

    public bool OnlyCritical
    {
        get => _onlyCritical;
        set
        {
            if (Set(ref _onlyCritical, value))
                _ = RefreshAsync();
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
            var summary = await _service.GetAsync();

            UserDisplayName = summary.UserDisplayName;
            OpenCount = summary.OpenCount;
            DueTodayCount = summary.DueTodayCount;
            OverdueCount = summary.OverdueCount;
            UrgentCount = summary.UrgentCount;
            CustomerFollowUpCount = summary.CustomerFollowUpCount;
            ProjectTaskCount = summary.ProjectTaskCount;

            Items.Clear();

            foreach (var item in summary.Items.Where(x =>
                         !OnlyCritical
                         || x.IsOverdue
                         || x.IsDueToday
                         || string.Equals(
                             x.Priority,
                             "Urgent",
                             StringComparison.OrdinalIgnoreCase)
                         || string.Equals(
                             x.Priority,
                             "High",
                             StringComparison.OrdinalIgnoreCase)))
            {
                Items.Add(item);
            }

            StatusText = $"{Items.Count} persönliche Arbeitspunkte angezeigt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "„Meine Arbeit“ konnte nicht geladen werden");
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
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;

        public bool CanExecute(object? parameter) => !_running;

        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (_running)
                return;

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
