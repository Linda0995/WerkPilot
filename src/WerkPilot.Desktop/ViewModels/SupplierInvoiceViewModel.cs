using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class SupplierInvoiceViewModel : INotifyPropertyChanged
{
    private readonly SupplierInvoiceService _service;
    private readonly SupplierOrderService _orders;
    private SupplierInvoiceDto? _selectedInvoice;
    private SupplierInvoiceLineDto? _selectedLine;
    private SupplierOrderDto? _selectedOrder;
    private string _invoiceNumber = string.Empty;
    private DateTimeOffset? _invoiceDate = DateTimeOffset.Now;
    private DateTimeOffset? _dueDate = DateTimeOffset.Now.AddDays(14);
    private decimal _invoicedQuantity;
    private decimal _unitPriceNet;
    private string? _reviewNote;
    private bool _allowWarnings;
    private decimal _cashDiscountPercent;
    private DateTimeOffset? _cashDiscountDueDate;
    private decimal _paymentAmount;
    private DateTimeOffset? _paymentDate = DateTimeOffset.Now;
    private string? _paymentReference;
    private string _statusText = "Bereit";

    public SupplierInvoiceViewModel(
        SupplierInvoiceService service,
        SupplierOrderService orders)
    {
        _service = service;
        _orders = orders;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        UpdateLineCommand = new AsyncCommand(UpdateLineAsync, HasLine);
        SubmitCommand = new AsyncCommand(SubmitAsync, HasInvoice);
        ApproveCommand = new AsyncCommand(ApproveAsync, HasInvoice);
        RejectCommand = new AsyncCommand(RejectAsync, HasInvoice);
        MarkPaidCommand = new AsyncCommand(MarkPaidAsync, HasInvoice);
        CancelCommand = new AsyncCommand(CancelAsync, HasInvoice);
        ExportCommand = new AsyncCommand(ExportAsync, HasInvoice);
        RegisterPaymentCommand = new AsyncCommand(RegisterPaymentAsync, CanRegisterPayment);
        RefreshLiquidityCommand = new AsyncCommand(RefreshLiquidityAsync);

        _ = InitializeAsync();
    }

    public ObservableCollection<SupplierInvoiceDto> Invoices { get; } = [];
    public ObservableCollection<SupplierInvoiceLineDto> Lines { get; } = [];
    public ObservableCollection<SupplierOrderDto> Orders { get; } = [];
    public ObservableCollection<SupplierInvoicePaymentDto> Payments { get; } = [];
    public ObservableCollection<SupplierInvoiceLiquidityItemDto> LiquidityItems { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateLineCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand RejectCommand { get; }
    public ICommand MarkPaidCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RegisterPaymentCommand { get; }
    public ICommand RefreshLiquidityCommand { get; }

    public SupplierInvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (Set(ref _selectedInvoice, value))
            {
                Lines.Clear();

                if (value is not null)
                    foreach (var line in value.Lines)
                        Lines.Add(line);

                SelectedLine = Lines.FirstOrDefault();
                ReviewNote = value?.ReviewNote;
                Payments.Clear();
                if (value is not null)
                    foreach (var payment in value.Payments)
                        Payments.Add(payment);

                PaymentAmount = value?.OpenAmount ?? 0m;
                RefreshCommands();
            }
        }
    }

    public SupplierInvoiceLineDto? SelectedLine
    {
        get => _selectedLine;
        set
        {
            if (Set(ref _selectedLine, value))
            {
                InvoicedQuantity = value?.InvoicedQuantity ?? 0m;
                UnitPriceNet = value?.InvoicedUnitPriceNet ?? 0m;
                RefreshCommands();
            }
        }
    }

    public SupplierOrderDto? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            Set(ref _selectedOrder, value);
            RefreshCommands();
        }
    }

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set
        {
            Set(ref _invoiceNumber, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? InvoiceDate { get => _invoiceDate; set { Set(ref _invoiceDate, value); RefreshCommands(); } }
    public DateTimeOffset? DueDate { get => _dueDate; set { Set(ref _dueDate, value); RefreshCommands(); } }
    public decimal InvoicedQuantity { get => _invoicedQuantity; set => Set(ref _invoicedQuantity, value); }
    public decimal UnitPriceNet { get => _unitPriceNet; set => Set(ref _unitPriceNet, value); }
    public string? ReviewNote { get => _reviewNote; set => Set(ref _reviewNote, value); }
    public bool AllowWarnings { get => _allowWarnings; set => Set(ref _allowWarnings, value); }
    public decimal CashDiscountPercent { get => _cashDiscountPercent; set => Set(ref _cashDiscountPercent, value); }
    public DateTimeOffset? CashDiscountDueDate { get => _cashDiscountDueDate; set => Set(ref _cashDiscountDueDate, value); }
    public decimal PaymentAmount { get => _paymentAmount; set { Set(ref _paymentAmount, value); RefreshCommands(); } }
    public DateTimeOffset? PaymentDate { get => _paymentDate; set { Set(ref _paymentDate, value); RefreshCommands(); } }
    public string? PaymentReference { get => _paymentReference; set => Set(ref _paymentReference, value); }

    public decimal TotalOpenAmount { get; private set; }
    public decimal OverdueAmount { get; private set; }
    public decimal DueWithin7Days { get; private set; }
    public decimal DueWithin14Days { get; private set; }
    public decimal DueWithin30Days { get; private set; }
    public decimal AvailableCashDiscount { get; private set; }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var order in await _orders.GetAllAsync())
            Orders.Add(order);

        SelectedOrder = Orders.FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedInvoice?.Id;
            Invoices.Clear();

            foreach (var invoice in await _service.GetAllAsync())
                Invoices.Add(invoice);

            SelectedInvoice = selectedId.HasValue
                ? Invoices.FirstOrDefault(x => x.Id == selectedId.Value)
                : Invoices.FirstOrDefault();

            await RefreshLiquidityAsync();
            StatusText = $"{Invoices.Count} Eingangsrechnung(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Eingangsrechnungen konnten nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedOrder is null || !InvoiceDate.HasValue || !DueDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateFromOrderAsync(
                SelectedOrder.Id,
                InvoiceNumber,
                DateOnly.FromDateTime(InvoiceDate.Value.DateTime),
                DateOnly.FromDateTime(DueDate.Value.DateTime),
                CashDiscountPercent,
                CashDiscountDueDate.HasValue
                    ? DateOnly.FromDateTime(CashDiscountDueDate.Value.DateTime)
                    : null);

            await RefreshAsync();
            SelectedInvoice = Invoices.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Eingangsrechnung {created.InvoiceNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Eingangsrechnung konnte nicht angelegt werden");
        }
    }

    private async Task UpdateLineAsync()
    {
        if (SelectedInvoice is null || SelectedLine is null)
            return;

        try
        {
            await _service.UpdateLineAsync(
                SelectedInvoice.Id,
                SelectedLine.Id,
                InvoicedQuantity,
                UnitPriceNet);

            await RefreshAsync();
            StatusText = "Rechnungsposition wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnungsposition konnte nicht aktualisiert werden");
        }
    }

    private async Task SubmitAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.SubmitForReviewAsync(SelectedInvoice.Id);
            await RefreshAsync();
            StatusText = "Rechnung wurde zur Prüfung eingereicht.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht eingereicht werden");
        }
    }

    private async Task ApproveAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.ApproveAsync(
                SelectedInvoice.Id,
                ReviewNote,
                AllowWarnings);

            await RefreshAsync();
            StatusText = "Rechnung wurde freigegeben.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht freigegeben werden");
        }
    }

    private async Task RejectAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.RejectAsync(
                SelectedInvoice.Id,
                ReviewNote ?? string.Empty);

            await RefreshAsync();
            StatusText = "Rechnung wurde abgelehnt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht abgelehnt werden");
        }
    }

    private async Task MarkPaidAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.MarkPaidAsync(SelectedInvoice.Id);
            await RefreshAsync();
            StatusText = "Rechnung wurde als bezahlt markiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht als bezahlt markiert werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedInvoice.Id);
            await RefreshAsync();
            StatusText = "Rechnung wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht storniert werden");
        }
    }

    private async Task ExportAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Eingangsrechnungen");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _service.ExportCsvAsync(SelectedInvoice.Id));

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = $"Prüfprotokoll wurde exportiert: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Prüfprotokoll konnte nicht exportiert werden");
        }
    }


    private async Task RegisterPaymentAsync()
    {
        if (SelectedInvoice is null || !PaymentDate.HasValue)
            return;

        try
        {
            await _service.RegisterPaymentAsync(
                SelectedInvoice.Id,
                PaymentAmount,
                DateOnly.FromDateTime(PaymentDate.Value.DateTime),
                PaymentReference);

            await RefreshAsync();
            StatusText = "Zahlung wurde erfasst.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zahlung konnte nicht erfasst werden");
        }
    }

    private async Task RefreshLiquidityAsync()
    {
        var summary = await _service.GetLiquiditySummaryAsync(
            DateOnly.FromDateTime(DateTime.Today));

        LiquidityItems.Clear();
        foreach (var item in summary.Items)
            LiquidityItems.Add(item);

        TotalOpenAmount = summary.TotalOpenAmount;
        OverdueAmount = summary.OverdueAmount;
        DueWithin7Days = summary.DueWithin7Days;
        DueWithin14Days = summary.DueWithin14Days;
        DueWithin30Days = summary.DueWithin30Days;
        AvailableCashDiscount = summary.AvailableCashDiscount;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalOpenAmount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverdueAmount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DueWithin7Days)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DueWithin14Days)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DueWithin30Days)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableCashDiscount)));
    }

    private bool CanCreate() =>
        SelectedOrder is not null &&
        !string.IsNullOrWhiteSpace(InvoiceNumber) &&
        InvoiceDate.HasValue &&
        DueDate.HasValue;

    private bool HasInvoice() => SelectedInvoice is not null;
    private bool HasLine() => SelectedInvoice is not null && SelectedLine is not null;
    private bool CanRegisterPayment() =>
        SelectedInvoice is not null &&
        PaymentDate.HasValue &&
        PaymentAmount > 0m &&
        PaymentAmount <= SelectedInvoice.OpenAmount;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateCommand,
            UpdateLineCommand,
            SubmitCommand,
            ApproveCommand,
            RejectCommand,
            MarkPaidCommand,
            CancelCommand,
            ExportCommand,
            RegisterPaymentCommand,
            RefreshLiquidityCommand
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
