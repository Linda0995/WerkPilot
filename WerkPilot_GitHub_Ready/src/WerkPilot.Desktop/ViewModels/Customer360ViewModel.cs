using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;

namespace WerkPilot.Desktop.ViewModels;

public sealed class Customer360ViewModel : INotifyPropertyChanged
{
    private readonly CustomerService _customers;
    private readonly Customer360Service _customer360;
    private readonly DocumentService _documents;
    private CustomerDto? _selectedCustomer;
    private DocumentFileDto? _selectedDocument;
    private decimal _openOfferVolumeNet;
    private int _activeProjectCount;
    private int _openFollowUpCount;
    private string _lastContactText = "–";
    private string _statusText = "Bereit";

    public Customer360ViewModel(
        CustomerService customers,
        Customer360Service customer360,
        DocumentService documents)
    {
        _customers = customers;
        _customer360 = customer360;
        _documents = documents;

        RefreshCommand = new AsyncCommand(RefreshAsync, () => SelectedCustomer is not null);
        OpenDocumentCommand = new AsyncCommand(OpenDocumentAsync, () => SelectedDocument is not null);
        _ = InitializeAsync();
    }

    public ObservableCollection<CustomerDto> Customers { get; } = [];
    public ObservableCollection<OfferDto> Offers { get; } = [];
    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<CustomerInteractionDto> Interactions { get; } = [];
    public ObservableCollection<CustomerInteractionDto> OpenFollowUps { get; } = [];
    public ObservableCollection<DocumentFileDto> Documents { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand OpenDocumentCommand { get; }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (Set(ref _selectedCustomer, value))
            {
                _ = RefreshAsync();
                (RefreshCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public DocumentFileDto? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            Set(ref _selectedDocument, value);
            (OpenDocumentCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        }
    }

    public decimal OpenOfferVolumeNet
    {
        get => _openOfferVolumeNet;
        private set => Set(ref _openOfferVolumeNet, value);
    }

    public int ActiveProjectCount
    {
        get => _activeProjectCount;
        private set => Set(ref _activeProjectCount, value);
    }

    public int OpenFollowUpCount
    {
        get => _openFollowUpCount;
        private set => Set(ref _openFollowUpCount, value);
    }

    public string LastContactText
    {
        get => _lastContactText;
        private set => Set(ref _lastContactText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task InitializeAsync()
    {
        foreach (var customer in await _customers.SearchAsync(null))
            Customers.Add(customer);

        SelectedCustomer = Customers.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        if (SelectedCustomer is null)
            return;

        try
        {
            var data = await _customer360.GetAsync(SelectedCustomer.Id);

            Offers.Clear();
            foreach (var item in data.Offers)
                Offers.Add(item);

            Projects.Clear();
            foreach (var item in data.Projects)
                Projects.Add(item);

            Interactions.Clear();
            foreach (var item in data.Interactions)
                Interactions.Add(item);

            OpenFollowUps.Clear();
            foreach (var item in data.OpenFollowUps)
                OpenFollowUps.Add(item);

            Documents.Clear();
            foreach (var item in data.Documents)
                Documents.Add(item);

            OpenOfferVolumeNet = data.OpenOfferVolumeNet;
            ActiveProjectCount = data.ActiveProjectCount;
            OpenFollowUpCount = data.OpenFollowUpCount;
            LastContactText = data.Customer.LastContactAtUtc.HasValue
                ? data.Customer.LastContactAtUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
                : "Noch kein Kontakt";

            StatusText =
                $"{data.Customer.DisplayName}: {Offers.Count} Angebot(e), "
                + $"{Projects.Count} Projekt(e), {Interactions.Count} CRM-Kontakt(e).";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Kundenübersicht konnte nicht geladen werden");
        }
    }

    private async Task OpenDocumentAsync()
    {
        if (SelectedDocument is null)
            return;

        try
        {
            var path = await _documents.GetAbsolutePathAsync(SelectedDocument.Id);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = "Dokument wurde geöffnet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Dokument konnte nicht geöffnet werden");
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
