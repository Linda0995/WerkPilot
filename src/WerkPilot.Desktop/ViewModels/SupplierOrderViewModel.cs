using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Inventory;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class SupplierOrderViewModel : INotifyPropertyChanged
{
    private readonly SupplierOrderService _service;
    private readonly ReorderSuggestionService _suggestions;
    private SupplierOrderDto? _selectedOrder;
    private SupplierOrderLineDto? _selectedLine;
    private string? _selectedSupplier;
    private DateTimeOffset? _orderDate = DateTimeOffset.Now;
    private DateTimeOffset? _expectedDeliveryDate = DateTimeOffset.Now.AddDays(7);
    private string? _supplierReference;
    private decimal _receiveQuantity;
    private string? _deliveryReference;
    private string _statusText = "Bereit";

    public SupplierOrderViewModel(
        SupplierOrderService service,
        ReorderSuggestionService suggestions)
    {
        _service = service;
        _suggestions = suggestions;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateFromSuggestionsCommand = new AsyncCommand(CreateFromSuggestionsAsync, CanCreate);
        MarkOrderedCommand = new AsyncCommand(MarkOrderedAsync, HasOrder);
        ReceiveCommand = new AsyncCommand(ReceiveAsync, HasLine);
        CancelCommand = new AsyncCommand(CancelAsync, HasOrder);
        ExportCommand = new AsyncCommand(ExportAsync, HasOrder);

        _ = InitializeAsync();
    }

    public ObservableCollection<SupplierOrderDto> Orders { get; } = [];
    public ObservableCollection<SupplierOrderLineDto> Lines { get; } = [];
    public ObservableCollection<string> Suppliers { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateFromSuggestionsCommand { get; }
    public ICommand MarkOrderedCommand { get; }
    public ICommand ReceiveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }

    public SupplierOrderDto? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (Set(ref _selectedOrder, value))
            {
                Lines.Clear();
                if (value is not null)
                    foreach (var line in value.Lines)
                        Lines.Add(line);

                SelectedLine = Lines.FirstOrDefault();
                RefreshCommands();
            }
        }
    }

    public SupplierOrderLineDto? SelectedLine
    {
        get => _selectedLine;
        set
        {
            if (Set(ref _selectedLine, value))
            {
                ReceiveQuantity = value?.OpenQuantity ?? 0m;
                RefreshCommands();
            }
        }
    }

    public string? SelectedSupplier
    {
        get => _selectedSupplier;
        set
        {
            Set(ref _selectedSupplier, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? OrderDate { get => _orderDate; set => Set(ref _orderDate, value); }
    public DateTimeOffset? ExpectedDeliveryDate { get => _expectedDeliveryDate; set => Set(ref _expectedDeliveryDate, value); }
    public string? SupplierReference { get => _supplierReference; set => Set(ref _supplierReference, value); }
    public decimal ReceiveQuantity { get => _receiveQuantity; set => Set(ref _receiveQuantity, value); }
    public string? DeliveryReference { get => _deliveryReference; set => Set(ref _deliveryReference, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        await RefreshSuppliersAsync();
        await RefreshAsync();
    }

    private async Task RefreshSuppliersAsync()
    {
        Suppliers.Clear();

        foreach (var supplier in (await _suggestions.GetAsync())
                     .Select(x => x.Supplier)
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(x => x))
        {
            Suppliers.Add(supplier!);
        }

        SelectedSupplier ??= Suppliers.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedOrder?.Id;
            Orders.Clear();

            foreach (var order in await _service.GetAllAsync())
                Orders.Add(order);

            SelectedOrder = selectedId.HasValue
                ? Orders.FirstOrDefault(x => x.Id == selectedId.Value)
                : Orders.FirstOrDefault();

            await RefreshSuppliersAsync();
            StatusText = $"{Orders.Count} Lieferantenbestellung(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellungen konnten nicht geladen werden");
        }
    }

    private async Task CreateFromSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedSupplier) || !OrderDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateFromSuggestionsAsync(
                SelectedSupplier,
                DateOnly.FromDateTime(OrderDate.Value.DateTime),
                ExpectedDeliveryDate.HasValue
                    ? DateOnly.FromDateTime(ExpectedDeliveryDate.Value.DateTime)
                    : null,
                SupplierReference);

            await RefreshAsync();
            SelectedOrder = Orders.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Bestellung {created.OrderNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellung konnte nicht angelegt werden");
        }
    }

    private async Task MarkOrderedAsync()
    {
        if (SelectedOrder is null)
            return;

        try
        {
            await _service.MarkOrderedAsync(SelectedOrder.Id);
            await RefreshAsync();
            StatusText = "Bestellung wurde als bestellt markiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellung konnte nicht ausgelöst werden");
        }
    }

    private async Task ReceiveAsync()
    {
        if (SelectedOrder is null || SelectedLine is null)
            return;

        try
        {
            await _service.ReceiveAsync(
                SelectedOrder.Id,
                SelectedLine.Id,
                ReceiveQuantity,
                DeliveryReference);

            await RefreshAsync();
            StatusText = "Wareneingang wurde gebucht und der Lagerbestand erhöht.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wareneingang konnte nicht gebucht werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedOrder is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedOrder.Id);
            await RefreshAsync();
            StatusText = "Bestellung wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellung konnte nicht storniert werden");
        }
    }

    private async Task ExportAsync()
    {
        if (SelectedOrder is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Bestellungen");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedOrder.OrderNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _service.ExportCsvAsync(SelectedOrder.Id));

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = $"Bestellung wurde exportiert: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellung konnte nicht exportiert werden");
        }
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(SelectedSupplier) && OrderDate.HasValue;

    private bool HasOrder() => SelectedOrder is not null;
    private bool HasLine() =>
        SelectedOrder is not null &&
        SelectedLine is not null &&
        ReceiveQuantity > 0;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateFromSuggestionsCommand,
            MarkOrderedCommand,
            ReceiveCommand,
            CancelCommand,
            ExportCommand
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
