using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CustomerFollowUpViewModel : INotifyPropertyChanged
{
    private readonly CustomerFollowUpService _service;
    private readonly CustomerService _customerService;
    private readonly UserService _userService;
    private CustomerDto? _selectedCustomer;
    private CustomerFollowUpDto? _selectedFollowUp;
    private string _title = string.Empty;
    private string? _notes;
    private DateTimeOffset? _dueAt = DateTimeOffset.Now.AddDays(1);
    private CustomerFollowUpPriority _priority = CustomerFollowUpPriority.Normal;
    private UserAssignmentOption? _selectedAssignee;
    private string? _assignedTo;
    private string? _completionNote;
    private bool _showCompleted;
    private string _statusText = "Bereit";

    public CustomerFollowUpViewModel(
        CustomerFollowUpService service,
        CustomerService customerService,
        UserService userService)
    {
        _service = service;
        _customerService = customerService;
        _userService = userService;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        StartCommand = new AsyncCommand(StartAsync, HasFollowUp);
        RescheduleCommand = new AsyncCommand(RescheduleAsync, HasFollowUp);
        CompleteCommand = new AsyncCommand(CompleteAsync, HasFollowUp);
        CancelCommand = new AsyncCommand(CancelAsync, HasFollowUp);

        _ = InitializeAsync();
    }

    public ObservableCollection<CustomerDto> Customers { get; } = [];
    public ObservableCollection<UserAssignmentOption> Assignees { get; } = [];
    public ObservableCollection<CustomerFollowUpDto> FollowUps { get; } = [];
    public IReadOnlyList<CustomerFollowUpPriority> Priorities { get; } =
        Enum.GetValues<CustomerFollowUpPriority>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand RescheduleCommand { get; }
    public ICommand CompleteCommand { get; }
    public ICommand CancelCommand { get; }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set { Set(ref _selectedCustomer, value); RefreshCommands(); }
    }

    public CustomerFollowUpDto? SelectedFollowUp
    {
        get => _selectedFollowUp;
        set
        {
            if (Set(ref _selectedFollowUp, value) && value is not null)
            {
                SelectedCustomer = Customers.FirstOrDefault(x => x.Id == value.CustomerId);
                Title = value.Title;
                Notes = value.Notes;
                DueAt = value.DueAtUtc.ToLocalTime();
                Priority = value.Priority;
                AssignedTo = value.AssignedTo;
                SelectedAssignee = value.AssignedUserId.HasValue
                    ? Assignees.FirstOrDefault(x => x.UserId == value.AssignedUserId)
                    : Assignees.FirstOrDefault(x =>
                        string.Equals(
                            x.DisplayName,
                            value.AssignedTo,
                            StringComparison.OrdinalIgnoreCase))
                      ?? UserAssignmentOption.Unassigned;
                CompletionNote = value.CompletionNote;
                RefreshCommands();
            }
        }
    }

    public string Title
    {
        get => _title;
        set { Set(ref _title, value); RefreshCommands(); }
    }

    public string? Notes
    {
        get => _notes;
        set => Set(ref _notes, value);
    }

    public DateTimeOffset? DueAt
    {
        get => _dueAt;
        set { Set(ref _dueAt, value); RefreshCommands(); }
    }

    public CustomerFollowUpPriority Priority
    {
        get => _priority;
        set => Set(ref _priority, value);
    }


    public UserAssignmentOption? SelectedAssignee
    {
        get => _selectedAssignee;
        set
        {
            if (Set(ref _selectedAssignee, value))
                AssignedTo = value?.UserId is null ? null : value.DisplayName;
        }
    }

    public string? AssignedTo
    {
        get => _assignedTo;
        set => Set(ref _assignedTo, value);
    }

    public string? CompletionNote
    {
        get => _completionNote;
        set => Set(ref _completionNote, value);
    }

    public bool ShowCompleted
    {
        get => _showCompleted;
        set
        {
            if (Set(ref _showCompleted, value))
                _ = RefreshAsync();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task InitializeAsync()
    {
        foreach (var customer in await _customerService.SearchAsync(
                     null,
                     includeDeleted: false))
        {
            Customers.Add(customer);
        }

        Assignees.Add(UserAssignmentOption.Unassigned);

        foreach (var user in (await _userService.GetAllAsync())
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.DisplayName))
        {
            Assignees.Add(new UserAssignmentOption(user.Id, user.DisplayName));
        }

        SelectedCustomer = Customers.FirstOrDefault();
        SelectedAssignee = Assignees.FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedFollowUp?.Id;
            var items = await _service.GetAllAsync(DateTimeOffset.UtcNow);

            FollowUps.Clear();

            foreach (var item in items.Where(x =>
                         ShowCompleted
                         || x.Status is not CustomerFollowUpStatus.Completed
                             and not CustomerFollowUpStatus.Cancelled))
            {
                FollowUps.Add(item);
            }

            SelectedFollowUp = selectedId.HasValue
                ? FollowUps.FirstOrDefault(x => x.Id == selectedId.Value)
                : FollowUps.FirstOrDefault();

            StatusText = $"{FollowUps.Count} Wiedervorlage(n) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlagen konnten nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedCustomer is null || !DueAt.HasValue)
            return;

        try
        {
            var created = await _service.CreateAsync(
                new CreateCustomerFollowUpRequest(
                    SelectedCustomer.Id,
                    Title,
                    Notes,
                    DueAt.Value,
                    Priority,
                    SelectedAssignee?.UserId,
                    AssignedTo));

            ClearEditor();
            await RefreshAsync();
            SelectedFollowUp = FollowUps.FirstOrDefault(x => x.Id == created.Id);
            StatusText = "Wiedervorlage wurde erstellt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlage konnte nicht erstellt werden");
        }
    }

    private async Task StartAsync()
    {
        if (SelectedFollowUp is null)
            return;

        try
        {
            await _service.StartAsync(SelectedFollowUp.Id);
            await RefreshAsync();
            StatusText = "Aufgabe wurde gestartet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Aufgabe konnte nicht gestartet werden");
        }
    }

    private async Task RescheduleAsync()
    {
        if (SelectedFollowUp is null || !DueAt.HasValue)
            return;

        try
        {
            await _service.RescheduleAsync(
                SelectedFollowUp.Id,
                new RescheduleCustomerFollowUpRequest(
                    DueAt.Value,
                    Priority,
                    SelectedAssignee?.UserId,
                    AssignedTo));

            await RefreshAsync();
            StatusText = "Wiedervorlage wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlage konnte nicht aktualisiert werden");
        }
    }

    private async Task CompleteAsync()
    {
        if (SelectedFollowUp is null)
            return;

        try
        {
            await _service.CompleteAsync(
                SelectedFollowUp.Id,
                CompletionNote);

            await RefreshAsync();
            StatusText = "Wiedervorlage wurde abgeschlossen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlage konnte nicht abgeschlossen werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedFollowUp is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedFollowUp.Id);
            await RefreshAsync();
            StatusText = "Wiedervorlage wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlage konnte nicht storniert werden");
        }
    }

    private bool CanCreate() =>
        SelectedCustomer is not null
        && DueAt.HasValue
        && !string.IsNullOrWhiteSpace(Title);

    private bool HasFollowUp() => SelectedFollowUp is not null;

    private void ClearEditor()
    {
        Title = string.Empty;
        Notes = null;
        DueAt = DateTimeOffset.Now.AddDays(1);
        Priority = CustomerFollowUpPriority.Normal;
        SelectedAssignee = Assignees.FirstOrDefault();
        AssignedTo = null;
        CompletionNote = null;
    }

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateCommand,
            StartCommand,
            RescheduleCommand,
            CompleteCommand,
            CancelCommand
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
