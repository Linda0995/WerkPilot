using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Customers;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CustomerCommunicationViewModel : INotifyPropertyChanged
{
    private readonly CustomerCommunicationService _service;
    private CustomerCommunicationSummaryDto? _selectedCustomer;
    private string _searchText = string.Empty;
    private string _statusText = "Bereit";

    public CustomerCommunicationViewModel(CustomerCommunicationService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<CustomerCommunicationSummaryDto> Customers { get; } = [];
    public ObservableCollection<CustomerCommunicationItemDto> Timeline { get; } = [];
    public ICommand RefreshCommand { get; }

    public CustomerCommunicationSummaryDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (Set(ref _selectedCustomer, value))
            {
                Timeline.Clear();

                if (value is not null)
                {
                    foreach (var item in value.Items)
                        Timeline.Add(item);
                }
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value))
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
            var selectedId = SelectedCustomer?.CustomerId;
            var all = await _service.GetAllAsync();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? all
                : all.Where(x =>
                    x.CustomerName.Contains(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase)
                    || x.CustomerNumber.Contains(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase)
                    || (x.Email?.Contains(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase) ?? false))
                  .ToArray();

            Customers.Clear();

            foreach (var customer in filtered)
                Customers.Add(customer);

            SelectedCustomer = selectedId.HasValue
                ? Customers.FirstOrDefault(x => x.CustomerId == selectedId.Value)
                : Customers.FirstOrDefault();

            StatusText =
                $"{Customers.Count} Kundenkommunikationsakten geladen.";
        }
        catch (Exception ex)
        {
            StatusText =
                UiErrorFormatter.Format(ex, "Kommunikationsakten konnten nicht geladen werden");
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
