using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Auditing;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Dashboard;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Notifications;
using WerkPilot.Domain.Customers;

namespace WerkPilot.Desktop.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly CustomerService _service;
    private readonly AuthenticationService _authenticationService;
    private readonly AuthorizationService _authorizationService;
    private readonly SessionContext _session;
    private readonly DashboardService _dashboardService;
    private readonly NotificationService _notificationService;
    private string _searchText = string.Empty;
    private string _newCustomerName = string.Empty;
    private string _statusText = "Bereit";
    private CustomerDto? _selectedCustomer;
    private CustomerContactDto? _selectedContact;
    private bool _includeDeleted;
    private int _customerCount;
    private int _favoriteCount;
    private int _openOfferCount;
    private decimal _openOfferVolumeNet;
    private int _activeProjectCount;
    private int _openProjectTaskCount;
    private int _dueTaskCount;
    private int _overdueTaskCount;
    private int _openCrmFollowUpCount;
    private int _overdueCrmFollowUpCount;
    private int _openCustomerFollowUpCount;
    private int _dueTodayCustomerFollowUpCount;
    private int _overdueCustomerFollowUpCount;
    private int _urgentCustomerFollowUpCount;
    private int _unreadNotificationCount;

    private string _editName = string.Empty;
    private string? _editContactPerson;
    private string? _billingStreet;
    private string? _billingPostalCode;
    private string? _billingCity;
    private string _billingCountryCode = "AT";
    private string? _deliveryStreet;
    private string? _deliveryPostalCode;
    private string? _deliveryCity;
    private string _deliveryCountryCode = "AT";
    private bool _deliveryEqualsBilling = true;
    private string? _editEmail;
    private string? _editPhone;
    private string? _editVatId;
    private string? _editNotes;
    private TaxProfile _editTaxProfile = TaxProfile.Inland;

    private string _newContactLabel = string.Empty;
    private string? _newContactEmail;
    private string? _newContactPhone;
    private bool _newContactIsPrimary;

    public MainWindowViewModel(
        CustomerService service,
        AuthenticationService authenticationService,
        AuthorizationService authorizationService,
        SessionContext session,
        DashboardService dashboardService,
        NotificationService notificationService,
        string? initialStatus = null)
    {
        _service = service;
        _authenticationService = authenticationService;
        _authorizationService = authorizationService;
        _session = session;
        _dashboardService = dashboardService;
        _notificationService = notificationService;
        if (!string.IsNullOrWhiteSpace(initialStatus))
            _statusText = initialStatus;

        SearchCommand = new AsyncCommand(RefreshAsync);
        AddCustomerCommand = new AsyncCommand(AddCustomerAsync, () => CanEditCustomers);
        SaveCustomerCommand = new AsyncCommand(SaveCustomerAsync, CanEditSelection);
        ToggleFavoriteCommand = new AsyncCommand(ToggleFavoriteAsync, CanEditSelection);
        MoveToTrashCommand = new AsyncCommand(MoveToTrashAsync, () => CanMoveToTrash() && CanEditCustomers);
        RestoreCommand = new AsyncCommand(RestoreAsync, () => CanRestore() && CanEditCustomers);
        AddContactCommand = new AsyncCommand(AddContactAsync, CanEditSelection);
        RemoveContactCommand = new AsyncCommand(RemoveContactAsync, CanEditSelectedContact);
        SetPrimaryContactCommand = new AsyncCommand(SetPrimaryContactAsync, CanEditSelectedContact);
        LogoutCommand = new RelayCommand(Logout);
        OpenOffersCommand = new RelayCommand(() => OpenOffersRequested?.Invoke(this, EventArgs.Empty));
        OpenCompanySettingsCommand = new RelayCommand(
            () => OpenCompanySettingsRequested?.Invoke(this, EventArgs.Empty));
        OpenCalculationCommand = new RelayCommand(
            () => OpenCalculationRequested?.Invoke(this, EventArgs.Empty));
        OpenMaterialCommand = new RelayCommand(
            () => OpenMaterialRequested?.Invoke(this, EventArgs.Empty));
        OpenPurchaseListsCommand = new RelayCommand(
            () => OpenPurchaseListsRequested?.Invoke(this, EventArgs.Empty));
        OpenProjectsCommand = new RelayCommand(
            () => OpenProjectsRequested?.Invoke(this, EventArgs.Empty));
        OpenDocumentsCommand = new RelayCommand(
            () => OpenDocumentsRequested?.Invoke(this, EventArgs.Empty));
        OpenNotificationsCommand = new RelayCommand(
            () => OpenNotificationsRequested?.Invoke(this, EventArgs.Empty));
        OpenGlobalSearchCommand = new RelayCommand(
            () => OpenGlobalSearchRequested?.Invoke(this, EventArgs.Empty));
        OpenWorkbenchCommand = new RelayCommand(
            () => OpenWorkbenchRequested?.Invoke(this, EventArgs.Empty));
        OpenCrmJournalCommand = new RelayCommand(
            () => OpenCrmJournalRequested?.Invoke(this, EventArgs.Empty));
        OpenCustomer360Command = new RelayCommand(
            () => OpenCustomer360Requested?.Invoke(this, EventArgs.Empty));
        OpenTimeTrackingCommand = new RelayCommand(
            () => OpenTimeTrackingRequested?.Invoke(this, EventArgs.Empty));
        OpenProjectCostControllingCommand = new RelayCommand(
            () => OpenProjectCostControllingRequested?.Invoke(this, EventArgs.Empty));
        OpenInventoryCommand = new RelayCommand(
            () => OpenInventoryRequested?.Invoke(this, EventArgs.Empty));
        OpenInventoryCountCommand = new RelayCommand(
            () => OpenInventoryCountRequested?.Invoke(this, EventArgs.Empty));
        OpenSupplierOrdersCommand = new RelayCommand(
            () => OpenSupplierOrdersRequested?.Invoke(this, EventArgs.Empty));
        OpenSupplierInvoicesCommand = new RelayCommand(
            () => OpenSupplierInvoicesRequested?.Invoke(this, EventArgs.Empty));
        OpenSupplierLiquidityCommand = new RelayCommand(
            () => OpenSupplierLiquidityRequested?.Invoke(this, EventArgs.Empty));
        OpenCustomerInvoicesCommand = new RelayCommand(
            () => OpenCustomerInvoicesRequested?.Invoke(this, EventArgs.Empty));
        OpenReceivablesCommand = new RelayCommand(
            () => OpenReceivablesRequested?.Invoke(this, EventArgs.Empty));
        OpenCustomerCreditNotesCommand = new RelayCommand(
            () => OpenCustomerCreditNotesRequested?.Invoke(this, EventArgs.Empty));
        OpenDunningNoticesCommand = new RelayCommand(
            () => OpenDunningNoticesRequested?.Invoke(this, EventArgs.Empty));
        OpenDocumentEmailCommand = new RelayCommand(
            () => OpenDocumentEmailRequested?.Invoke(this, EventArgs.Empty));
        OpenCustomerCommunicationCommand = new RelayCommand(
            () => OpenCustomerCommunicationRequested?.Invoke(this, EventArgs.Empty));
        OpenCustomerFollowUpsCommand = new RelayCommand(
            () => OpenCustomerFollowUpsRequested?.Invoke(this, EventArgs.Empty));
        OpenMyWorkCommand = new RelayCommand(
            () => OpenMyWorkRequested?.Invoke(this, EventArgs.Empty));
        OpenTeamWorkCommand = new RelayCommand(
            () => OpenTeamWorkRequested?.Invoke(this, EventArgs.Empty));
        OpenWorkReassignmentCommand = new RelayCommand(
            () => OpenWorkReassignmentRequested?.Invoke(this, EventArgs.Empty));
        OpenUserAbsencesCommand = new RelayCommand(
            () => OpenUserAbsencesRequested?.Invoke(this, EventArgs.Empty));
        OpenBasicWorkflowAuditCommand = new RelayCommand(
            () => OpenBasicWorkflowAuditRequested?.Invoke(this, EventArgs.Empty));
        OpenReleaseDiagnosticsCommand = new RelayCommand(
            () => OpenReleaseDiagnosticsRequested?.Invoke(this, EventArgs.Empty));
        OpenFirstRunReadinessCommand = new RelayCommand(
            () => OpenFirstRunReadinessRequested?.Invoke(this, EventArgs.Empty));

        _ = RefreshAsync();
    }

    public ObservableCollection<CustomerDto> Customers { get; } = [];
    public ObservableCollection<CustomerContactDto> Contacts { get; } = [];
    public ObservableCollection<AuditEvent> History { get; } = [];
    public ObservableCollection<DashboardTaskItem> DueTasks { get; } = [];
    public ObservableCollection<DashboardCrmFollowUpItem> CrmFollowUps { get; } = [];
    public ObservableCollection<DashboardCustomerFollowUpItem> CustomerFollowUps { get; } = [];
    public ObservableCollection<DashboardActivityItem> RecentItems { get; } = [];
    public IReadOnlyList<TaxProfile> TaxProfiles { get; } = Enum.GetValues<TaxProfile>();

    public ICommand SearchCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public ICommand SaveCustomerCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand MoveToTrashCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand AddContactCommand { get; }
    public ICommand RemoveContactCommand { get; }
    public ICommand SetPrimaryContactCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand OpenOffersCommand { get; }
    public ICommand OpenCompanySettingsCommand { get; }
    public ICommand OpenCalculationCommand { get; }
    public ICommand OpenMaterialCommand { get; }
    public ICommand OpenPurchaseListsCommand { get; }
    public ICommand OpenProjectsCommand { get; }
    public ICommand OpenDocumentsCommand { get; }
    public ICommand OpenNotificationsCommand { get; }
    public ICommand OpenGlobalSearchCommand { get; }
    public ICommand OpenWorkbenchCommand { get; }
    public ICommand OpenCrmJournalCommand { get; }
    public ICommand OpenCustomer360Command { get; }
    public ICommand OpenTimeTrackingCommand { get; }
    public ICommand OpenProjectCostControllingCommand { get; }
    public ICommand OpenInventoryCommand { get; }
    public ICommand OpenInventoryCountCommand { get; }
    public ICommand OpenSupplierOrdersCommand { get; }
    public ICommand OpenSupplierInvoicesCommand { get; }
    public ICommand OpenSupplierLiquidityCommand { get; }
    public ICommand OpenCustomerInvoicesCommand { get; }
    public ICommand OpenReceivablesCommand { get; }
    public ICommand OpenCustomerCreditNotesCommand { get; }
    public ICommand OpenDunningNoticesCommand { get; }
    public ICommand OpenDocumentEmailCommand { get; }
    public ICommand OpenCustomerCommunicationCommand { get; }
    public ICommand OpenCustomerFollowUpsCommand { get; }
    public ICommand OpenMyWorkCommand { get; }
    public ICommand OpenTeamWorkCommand { get; }
    public ICommand OpenWorkReassignmentCommand { get; }
    public ICommand OpenUserAbsencesCommand { get; }
    public ICommand OpenBasicWorkflowAuditCommand { get; }
    public ICommand OpenReleaseDiagnosticsCommand { get; }
    public ICommand OpenFirstRunReadinessCommand { get; }

    public string SearchText { get => _searchText; set => Set(ref _searchText, value); }
    public string NewCustomerName { get => _newCustomerName; set => Set(ref _newCustomerName, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }
    public int CustomerCount { get => _customerCount; private set => Set(ref _customerCount, value); }
    public int FavoriteCount { get => _favoriteCount; private set => Set(ref _favoriteCount, value); }
    public int OpenOfferCount { get => _openOfferCount; private set => Set(ref _openOfferCount, value); }
    public decimal OpenOfferVolumeNet { get => _openOfferVolumeNet; private set => Set(ref _openOfferVolumeNet, value); }
    public int ActiveProjectCount { get => _activeProjectCount; private set => Set(ref _activeProjectCount, value); }
    public int OpenProjectTaskCount { get => _openProjectTaskCount; private set => Set(ref _openProjectTaskCount, value); }
    public int DueTaskCount { get => _dueTaskCount; private set => Set(ref _dueTaskCount, value); }
    public int OverdueTaskCount { get => _overdueTaskCount; private set => Set(ref _overdueTaskCount, value); }
    public int OpenCrmFollowUpCount { get => _openCrmFollowUpCount; private set => Set(ref _openCrmFollowUpCount, value); }
    public int OverdueCrmFollowUpCount { get => _overdueCrmFollowUpCount; private set => Set(ref _overdueCrmFollowUpCount, value); }
    public int OpenCustomerFollowUpCount { get => _openCustomerFollowUpCount; private set => Set(ref _openCustomerFollowUpCount, value); }
    public int DueTodayCustomerFollowUpCount { get => _dueTodayCustomerFollowUpCount; private set => Set(ref _dueTodayCustomerFollowUpCount, value); }
    public int OverdueCustomerFollowUpCount { get => _overdueCustomerFollowUpCount; private set => Set(ref _overdueCustomerFollowUpCount, value); }
    public int UrgentCustomerFollowUpCount { get => _urgentCustomerFollowUpCount; private set => Set(ref _urgentCustomerFollowUpCount, value); }
    public int UnreadNotificationCount { get => _unreadNotificationCount; private set => Set(ref _unreadNotificationCount, value); }
    public string SignedInUser => _session.DisplayName ?? "Nicht angemeldet";
    public string SignedInRole => _session.Role?.ToString() ?? "-";
    public bool CanEditCustomers => _authorizationService.CanEditCustomers();

    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set
        {
            if (Set(ref _includeDeleted, value))
                _ = RefreshAsync();
        }
    }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (!Set(ref _selectedCustomer, value))
                return;

            LoadEditor(value);
            RefreshCommands();
        }
    }

    public CustomerContactDto? SelectedContact
    {
        get => _selectedContact;
        set
        {
            Set(ref _selectedContact, value);
            RefreshCommands();
        }
    }

    public string EditName { get => _editName; set => Set(ref _editName, value); }
    public string? EditContactPerson { get => _editContactPerson; set => Set(ref _editContactPerson, value); }
    public string? BillingStreet { get => _billingStreet; set => Set(ref _billingStreet, value); }
    public string? BillingPostalCode { get => _billingPostalCode; set => Set(ref _billingPostalCode, value); }
    public string? BillingCity { get => _billingCity; set => Set(ref _billingCity, value); }
    public string BillingCountryCode { get => _billingCountryCode; set => Set(ref _billingCountryCode, value); }
    public string? DeliveryStreet { get => _deliveryStreet; set => Set(ref _deliveryStreet, value); }
    public string? DeliveryPostalCode { get => _deliveryPostalCode; set => Set(ref _deliveryPostalCode, value); }
    public string? DeliveryCity { get => _deliveryCity; set => Set(ref _deliveryCity, value); }
    public string DeliveryCountryCode { get => _deliveryCountryCode; set => Set(ref _deliveryCountryCode, value); }
    public bool DeliveryEqualsBilling { get => _deliveryEqualsBilling; set => Set(ref _deliveryEqualsBilling, value); }
    public string? EditEmail { get => _editEmail; set => Set(ref _editEmail, value); }
    public string? EditPhone { get => _editPhone; set => Set(ref _editPhone, value); }
    public string? EditVatId { get => _editVatId; set => Set(ref _editVatId, value); }
    public string? EditNotes { get => _editNotes; set => Set(ref _editNotes, value); }
    public TaxProfile EditTaxProfile { get => _editTaxProfile; set => Set(ref _editTaxProfile, value); }

    public string NewContactLabel { get => _newContactLabel; set => Set(ref _newContactLabel, value); }
    public string? NewContactEmail { get => _newContactEmail; set => Set(ref _newContactEmail, value); }
    public string? NewContactPhone { get => _newContactPhone; set => Set(ref _newContactPhone, value); }
    public bool NewContactIsPrimary { get => _newContactIsPrimary; set => Set(ref _newContactIsPrimary, value); }

    private async Task RefreshAsync()
    {
        try
        {
            StatusText = "Daten werden geladen …";
            var selectedId = SelectedCustomer?.Id;
            var list = await _service.SearchAsync(SearchText, IncludeDeleted);

            Customers.Clear();
            foreach (var item in list)
                Customers.Add(item);

            SelectedCustomer = selectedId is null
                ? null
                : Customers.FirstOrDefault(x => x.Id == selectedId);

            var dashboard = await _service.GetDashboardAsync();
            CustomerCount = dashboard.CustomerCount;
            FavoriteCount = dashboard.FavoriteCustomerCount;

            var operationalDashboard = await _dashboardService.GetAsync(
                DateOnly.FromDateTime(DateTime.Today));

            OpenOfferCount = operationalDashboard.OpenOfferCount;
            OpenOfferVolumeNet = operationalDashboard.OpenOfferVolumeNet;
            ActiveProjectCount = operationalDashboard.ActiveProjectCount;
            OpenProjectTaskCount = operationalDashboard.OpenProjectTaskCount;
            DueTaskCount = operationalDashboard.DueTaskCount;
            OverdueTaskCount = operationalDashboard.OverdueTaskCount;
            OpenCrmFollowUpCount = operationalDashboard.OpenCrmFollowUpCount;
            OverdueCrmFollowUpCount = operationalDashboard.OverdueCrmFollowUpCount;
            OpenCustomerFollowUpCount = operationalDashboard.OpenCustomerFollowUpCount;
            DueTodayCustomerFollowUpCount = operationalDashboard.DueTodayCustomerFollowUpCount;
            OverdueCustomerFollowUpCount = operationalDashboard.OverdueCustomerFollowUpCount;
            UrgentCustomerFollowUpCount = operationalDashboard.UrgentCustomerFollowUpCount;

            DueTasks.Clear();
            foreach (var task in operationalDashboard.DueTasks)
                DueTasks.Add(task);

            CrmFollowUps.Clear();
            foreach (var followUp in operationalDashboard.CrmFollowUps)
                CrmFollowUps.Add(followUp);

            CustomerFollowUps.Clear();
            foreach (var followUp in operationalDashboard.CustomerFollowUps)
                CustomerFollowUps.Add(followUp);

            RecentItems.Clear();
            foreach (var item in operationalDashboard.RecentItems)
                RecentItems.Add(item);

            var notifications = await _notificationService.GetAsync(DateOnly.FromDateTime(DateTime.Today));
            UnreadNotificationCount = notifications.Count(x => !x.IsRead);

            StatusText = $"{Customers.Count} Kunde(n) angezeigt";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Fehler");
        }
    }

    private async Task AddCustomerAsync()
    {
        try
        {
            var created = await _service.CreateAsync(NewCustomerName, CustomerType.Company);
            NewCustomerName = string.Empty;
            await RefreshAsync();
            SelectedCustomer = Customers.FirstOrDefault(x => x.Id == created.Id);
        }
        catch (CustomerValidationException ex)
        {
            StatusText = ex.ValidationResult.ToDisplayText();
        }
        catch (CustomerDuplicateException ex)
        {
            StatusText = FormatDuplicates(ex);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Kunde konnte nicht angelegt werden");
        }
    }

    private async Task SaveCustomerAsync()
    {
        if (SelectedCustomer is null) return;

        try
        {
            await _service.UpdateAsync(new UpdateCustomerRequest(
                SelectedCustomer.Id,
                EditName,
                SelectedCustomer.Type,
                EditContactPerson,
                BillingStreet,
                BillingPostalCode,
                BillingCity,
                BillingCountryCode,
                DeliveryStreet,
                DeliveryPostalCode,
                DeliveryCity,
                DeliveryCountryCode,
                DeliveryEqualsBilling,
                EditEmail,
                EditPhone,
                EditVatId,
                EditTaxProfile,
                EditNotes));

            await RefreshAsync();
            StatusText = "Kundendaten wurden gespeichert.";
        }
        catch (CustomerValidationException ex)
        {
            StatusText = ex.ValidationResult.ToDisplayText();
        }
        catch (CustomerDuplicateException ex)
        {
            StatusText = FormatDuplicates(ex);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Speichern fehlgeschlagen");
        }
    }

    private async Task AddContactAsync()
    {
        if (SelectedCustomer is null) return;

        try
        {
            await _service.AddContactAsync(new AddCustomerContactRequest(
                SelectedCustomer.Id,
                NewContactLabel,
                NewContactEmail,
                NewContactPhone,
                NewContactIsPrimary));

            NewContactLabel = string.Empty;
            NewContactEmail = null;
            NewContactPhone = null;
            NewContactIsPrimary = false;
            await RefreshAsync();
            StatusText = "Ansprechpartner wurde hinzugefügt.";
        }
        catch (CustomerValidationException ex)
        {
            StatusText = ex.ValidationResult.ToDisplayText();
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ansprechpartner konnte nicht angelegt werden");
        }
    }

    private async Task RemoveContactAsync()
    {
        if (SelectedCustomer is null || SelectedContact is null) return;
        await _service.RemoveContactAsync(SelectedCustomer.Id, SelectedContact.Id);
        await RefreshAsync();
        StatusText = "Ansprechpartner wurde entfernt.";
    }

    private async Task SetPrimaryContactAsync()
    {
        if (SelectedCustomer is null || SelectedContact is null) return;
        await _service.SetPrimaryContactAsync(SelectedCustomer.Id, SelectedContact.Id);
        await RefreshAsync();
        StatusText = "Hauptansprechpartner wurde übernommen.";
    }

    private async Task ToggleFavoriteAsync()
    {
        if (SelectedCustomer is null) return;
        await _service.ToggleFavoriteAsync(SelectedCustomer.Id);
        await RefreshAsync();
    }

    private async Task MoveToTrashAsync()
    {
        if (SelectedCustomer is null) return;
        await _service.MoveToTrashAsync(SelectedCustomer.Id);
        SelectedCustomer = null;
        await RefreshAsync();
    }

    private async Task RestoreAsync()
    {
        if (SelectedCustomer is null) return;
        await _service.RestoreAsync(SelectedCustomer.Id);
        await RefreshAsync();
        StatusText = "Kunde wurde wiederhergestellt.";
    }

    private void LoadEditor(CustomerDto? customer)
    {
        EditName = customer?.DisplayName ?? string.Empty;
        EditContactPerson = customer?.ContactPerson;
        BillingStreet = customer?.BillingStreet;
        BillingPostalCode = customer?.BillingPostalCode;
        BillingCity = customer?.BillingCity;
        BillingCountryCode = customer?.BillingCountryCode ?? "AT";
        DeliveryStreet = customer?.DeliveryStreet;
        DeliveryPostalCode = customer?.DeliveryPostalCode;
        DeliveryCity = customer?.DeliveryCity;
        DeliveryCountryCode = customer?.DeliveryCountryCode ?? "AT";
        DeliveryEqualsBilling =
            string.Equals(BillingStreet, DeliveryStreet, StringComparison.Ordinal) &&
            string.Equals(BillingPostalCode, DeliveryPostalCode, StringComparison.Ordinal) &&
            string.Equals(BillingCity, DeliveryCity, StringComparison.Ordinal) &&
            string.Equals(BillingCountryCode, DeliveryCountryCode, StringComparison.Ordinal);
        EditEmail = customer?.Email;
        EditPhone = customer?.Phone;
        EditVatId = customer?.VatId;
        EditTaxProfile = customer?.TaxProfile ?? TaxProfile.Inland;
        EditNotes = customer?.Notes;

        Contacts.Clear();
        if (customer is not null)
            foreach (var contact in customer.Contacts)
                Contacts.Add(contact);

        SelectedContact = null;
        _ = LoadHistoryAsync(customer?.Id);
    }

    private async Task LoadHistoryAsync(Guid? customerId)
    {
        History.Clear();
        if (!customerId.HasValue)
            return;

        try
        {
            var entries = await _service.GetHistoryAsync(customerId.Value);
            foreach (var entry in entries)
                History.Add(entry);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Änderungsverlauf konnte nicht geladen werden");
        }
    }

    private static string FormatDuplicates(CustomerDuplicateException exception)
    {
        var details = string.Join(
            "; ",
            exception.Duplicates.Select(x => $"{x.CustomerNumber} – {x.DisplayName} ({x.Reason})"));

        return $"Mögliche Dublette: {details}";
    }

    private bool HasSelection() => SelectedCustomer is not null;
    private bool CanEditSelection() => HasSelection() && CanEditCustomers;
    private bool CanEditSelectedContact() => HasSelectedContact() && CanEditCustomers;
    private bool HasSelectedContact() => SelectedCustomer is not null && SelectedContact is not null;
    private bool CanMoveToTrash() => SelectedCustomer is { IsDeleted: false };
    private bool CanRestore() => SelectedCustomer is { IsDeleted: true };

    private void Logout()
    {
        _authenticationService.Logout();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? LogoutRequested;
    public event EventHandler? OpenOffersRequested;
    public event EventHandler? OpenCompanySettingsRequested;
    public event EventHandler? OpenCalculationRequested;
    public event EventHandler? OpenMaterialRequested;
    public event EventHandler? OpenPurchaseListsRequested;
    public event EventHandler? OpenProjectsRequested;
    public event EventHandler? OpenDocumentsRequested;
    public event EventHandler? OpenNotificationsRequested;
    public event EventHandler? OpenGlobalSearchRequested;
    public event EventHandler? OpenWorkbenchRequested;
    public event EventHandler? OpenCrmJournalRequested;
    public event EventHandler? OpenCustomer360Requested;
    public event EventHandler? OpenTimeTrackingRequested;
    public event EventHandler? OpenProjectCostControllingRequested;
    public event EventHandler? OpenInventoryRequested;
    public event EventHandler? OpenInventoryCountRequested;
    public event EventHandler? OpenSupplierOrdersRequested;
    public event EventHandler? OpenSupplierInvoicesRequested;
    public event EventHandler? OpenSupplierLiquidityRequested;
    public event EventHandler? OpenCustomerInvoicesRequested;
    public event EventHandler? OpenReceivablesRequested;
    public event EventHandler? OpenCustomerCreditNotesRequested;
    public event EventHandler? OpenDunningNoticesRequested;
    public event EventHandler? OpenDocumentEmailRequested;
    public event EventHandler? OpenCustomerCommunicationRequested;
    public event EventHandler? OpenCustomerFollowUpsRequested;
    public event EventHandler? OpenMyWorkRequested;
    public event EventHandler? OpenTeamWorkRequested;
    public event EventHandler? OpenWorkReassignmentRequested;
    public event EventHandler? OpenUserAbsencesRequested;
    public event EventHandler? OpenBasicWorkflowAuditRequested;
    public event EventHandler? OpenReleaseDiagnosticsRequested;
    public event EventHandler? OpenFirstRunReadinessRequested;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            SaveCustomerCommand, ToggleFavoriteCommand, MoveToTrashCommand,
            RestoreCommand, AddContactCommand, RemoveContactCommand, SetPrimaryContactCommand
        })
            (command as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object? parameter) => execute();
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

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
