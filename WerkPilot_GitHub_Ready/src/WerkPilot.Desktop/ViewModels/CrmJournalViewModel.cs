using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Domain.Crm;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CrmJournalViewModel : INotifyPropertyChanged
{
    private readonly CustomerInteractionService _service;
    private readonly CustomerService _customers;
    private CustomerDto? _selectedCustomer;
    private CustomerInteractionDto? _selectedInteraction;
    private CustomerInteractionType _interactionType = CustomerInteractionType.Phone;
    private string _subject = string.Empty;
    private string _notes = string.Empty;
    private string? _contactPerson;
    private DateTimeOffset? _occurredAt = DateTimeOffset.Now;
    private DateTimeOffset? _followUpDate;
    private string? _followUpOwner;
    private bool _showOpenFollowUps;
    private string _statusText = "Bereit";

    public CrmJournalViewModel(
        CustomerInteractionService service,
        CustomerService customers)
    {
        _service = service;
        _customers = customers;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        UpdateCommand = new AsyncCommand(UpdateAsync, HasSelection);
        ToggleFollowUpCommand = new AsyncCommand(ToggleFollowUpAsync, HasSelection);
        _ = InitializeAsync();
    }

    public ObservableCollection<CustomerDto> Customers { get; } = [];
    public ObservableCollection<CustomerInteractionDto> Interactions { get; } = [];
    public IReadOnlyList<CustomerInteractionType> InteractionTypes { get; } =
        Enum.GetValues<CustomerInteractionType>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand ToggleFollowUpCommand { get; }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (Set(ref _selectedCustomer, value))
            {
                ContactPerson = value?.ContactPerson;
                _ = RefreshAsync();
                RefreshCommands();
            }
        }
    }

    public CustomerInteractionDto? SelectedInteraction
    {
        get => _selectedInteraction;
        set
        {
            if (Set(ref _selectedInteraction, value) && value is not null)
            {
                InteractionType = value.InteractionType;
                Subject = value.Subject;
                Notes = value.Notes;
                ContactPerson = value.ContactPerson;
                OccurredAt = value.OccurredAtUtc.ToLocalTime();
                FollowUpDate = value.FollowUpDate.HasValue
                    ? new DateTimeOffset(value.FollowUpDate.Value.ToDateTime(TimeOnly.MinValue))
                    : null;
                FollowUpOwner = value.FollowUpOwner;
            }

            RefreshCommands();
        }
    }

    public CustomerInteractionType InteractionType { get => _interactionType; set => Set(ref _interactionType, value); }
    public string Subject { get => _subject; set { Set(ref _subject, value); RefreshCommands(); } }
    public string Notes { get => _notes; set { Set(ref _notes, value); RefreshCommands(); } }
    public string? ContactPerson { get => _contactPerson; set => Set(ref _contactPerson, value); }
    public DateTimeOffset? OccurredAt { get => _occurredAt; set => Set(ref _occurredAt, value); }
    public DateTimeOffset? FollowUpDate { get => _followUpDate; set => Set(ref _followUpDate, value); }
    public string? FollowUpOwner { get => _followUpOwner; set => Set(ref _followUpOwner, value); }

    public bool ShowOpenFollowUps
    {
        get => _showOpenFollowUps;
        set
        {
            if (Set(ref _showOpenFollowUps, value))
                _ = RefreshAsync();
        }
    }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var customer in await _customers.SearchAsync(null))
            Customers.Add(customer);

        SelectedCustomer = Customers.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        Interactions.Clear();

        try
        {
            IReadOnlyList<CustomerInteractionDto> entries;

            if (ShowOpenFollowUps)
            {
                entries = await _service.GetOpenFollowUpsAsync(
                    DateOnly.FromDateTime(DateTime.Today.AddDays(30)));
            }
            else if (SelectedCustomer is not null)
            {
                entries = await _service.GetForCustomerAsync(SelectedCustomer.Id);
            }
            else
            {
                entries = [];
            }

            foreach (var entry in entries)
                Interactions.Add(entry);

            StatusText = $"{Interactions.Count} CRM-Eintrag/Einträge geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CRM-Journal konnte nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedCustomer is null) return;

        try
        {
            await _service.CreateAsync(
                SelectedCustomer.Id,
                InteractionType,
                Subject,
                Notes,
                (OccurredAt ?? DateTimeOffset.Now).ToUniversalTime(),
                ContactPerson,
                FollowUpDate.HasValue
                    ? DateOnly.FromDateTime(FollowUpDate.Value.DateTime)
                    : null,
                FollowUpOwner);

            ClearEditor();
            await ReloadCustomersAsync();
            await RefreshAsync();
            StatusText = "CRM-Kontakt wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CRM-Kontakt konnte nicht gespeichert werden");
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedInteraction is null) return;

        try
        {
            await _service.UpdateAsync(
                SelectedInteraction.Id,
                InteractionType,
                Subject,
                Notes,
                (OccurredAt ?? DateTimeOffset.Now).ToUniversalTime(),
                ContactPerson,
                FollowUpDate.HasValue
                    ? DateOnly.FromDateTime(FollowUpDate.Value.DateTime)
                    : null,
                FollowUpOwner);

            await ReloadCustomersAsync();
            await RefreshAsync();
            StatusText = "CRM-Kontakt wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CRM-Kontakt konnte nicht aktualisiert werden");
        }
    }

    private async Task ToggleFollowUpAsync()
    {
        if (SelectedInteraction is null) return;

        try
        {
            await _service.ToggleFollowUpCompletedAsync(SelectedInteraction.Id);
            await RefreshAsync();
            StatusText = "Wiedervorlagenstatus wurde geändert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiedervorlage konnte nicht geändert werden");
        }
    }

    private async Task ReloadCustomersAsync()
    {
        var selectedId = SelectedCustomer?.Id;
        Customers.Clear();

        foreach (var customer in await _customers.SearchAsync(null))
            Customers.Add(customer);

        SelectedCustomer = Customers.FirstOrDefault(x => x.Id == selectedId);
    }

    private void ClearEditor()
    {
        SelectedInteraction = null;
        InteractionType = CustomerInteractionType.Phone;
        Subject = string.Empty;
        Notes = string.Empty;
        ContactPerson = SelectedCustomer?.ContactPerson;
        OccurredAt = DateTimeOffset.Now;
        FollowUpDate = null;
        FollowUpOwner = null;
    }

    private bool CanCreate() =>
        SelectedCustomer is not null &&
        !string.IsNullOrWhiteSpace(Subject) &&
        !string.IsNullOrWhiteSpace(Notes);

    private bool HasSelection() => SelectedInteraction is not null;

    private void RefreshCommands()
    {
        (CreateCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (UpdateCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (ToggleFollowUpCommand as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running && (canExecute?.Invoke() ?? true);
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
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
