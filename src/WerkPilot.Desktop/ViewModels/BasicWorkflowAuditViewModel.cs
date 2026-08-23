using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Release;

namespace WerkPilot.Desktop.ViewModels;

public sealed class BasicWorkflowAuditViewModel : INotifyPropertyChanged
{
    private readonly BasicWorkflowAuditService _service;
    private int _customerCount;
    private int _offerCount;
    private int _completedCount;
    private int _issueCount;
    private int _orphanCount;
    private bool _onlyIssues;
    private string _statusText = "Bereit";

    public BasicWorkflowAuditViewModel(BasicWorkflowAuditService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<BasicWorkflowItemDto> Workflows { get; } = [];
    public ObservableCollection<BasicWorkflowOrphanDto> Orphans { get; } = [];
    public ICommand RefreshCommand { get; }

    public int CustomerCount
    {
        get => _customerCount;
        private set => Set(ref _customerCount, value);
    }

    public int OfferCount
    {
        get => _offerCount;
        private set => Set(ref _offerCount, value);
    }

    public int CompletedCount
    {
        get => _completedCount;
        private set => Set(ref _completedCount, value);
    }

    public int IssueCount
    {
        get => _issueCount;
        private set => Set(ref _issueCount, value);
    }

    public int OrphanCount
    {
        get => _orphanCount;
        private set => Set(ref _orphanCount, value);
    }

    public bool OnlyIssues
    {
        get => _onlyIssues;
        set
        {
            if (Set(ref _onlyIssues, value))
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
            var audit = await _service.EvaluateAsync(
                DateOnly.FromDateTime(DateTime.Today));

            CustomerCount = audit.CustomerCount;
            OfferCount = audit.OfferCount;
            CompletedCount = audit.CompletedWorkflowCount;
            IssueCount = audit.IssueCount;
            OrphanCount = audit.OrphanCount;

            Workflows.Clear();

            foreach (var item in audit.Workflows.Where(x =>
                         !OnlyIssues || x.HasIssue))
            {
                Workflows.Add(item);
            }

            Orphans.Clear();
            foreach (var item in audit.Orphans)
                Orphans.Add(item);

            StatusText = audit.IsHealthy
                ? "Basic-Prozessprüfung: keine Referenz- oder Workflowprobleme gefunden."
                : $"Basic-Prozessprüfung: {audit.IssueCount} Workflowproblem(e), "
                  + $"{audit.OrphanCount} verwaiste Referenz(en).";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Basic-Prozessprüfung fehlgeschlagen");
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
