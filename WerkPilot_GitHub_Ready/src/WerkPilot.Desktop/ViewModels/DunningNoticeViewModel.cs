using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class DunningNoticeViewModel : INotifyPropertyChanged
{
    private readonly DunningNoticeService _service;
    private readonly CustomerInvoiceService _invoices;
    private DunningNoticeDto? _selectedNotice;
    private CustomerInvoiceDto? _selectedInvoice;
    private DateTimeOffset? _noticeDate = DateTimeOffset.Now;
    private int _paymentTermDays = 7;
    private decimal _feeAmount;
    private decimal _annualInterestRatePercent = 9.2m;
    private string _statusText = "Bereit";

    public DunningNoticeViewModel(
        DunningNoticeService service,
        CustomerInvoiceService invoices)
    {
        _service = service;
        _invoices = invoices;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        IssueCommand = new AsyncCommand(IssueAsync, HasNotice);
        CancelCommand = new AsyncCommand(CancelAsync, HasNotice);
        ExportPdfCommand = new AsyncCommand(ExportPdfAsync, HasNotice);

        _ = RefreshAsync();
    }

    public ObservableCollection<DunningNoticeDto> Notices { get; } = [];
    public ObservableCollection<CustomerInvoiceDto> OverdueInvoices { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand IssueCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public DunningNoticeDto? SelectedNotice
    {
        get => _selectedNotice;
        set
        {
            Set(ref _selectedNotice, value);
            RefreshCommands();
        }
    }

    public CustomerInvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            Set(ref _selectedInvoice, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? NoticeDate
    {
        get => _noticeDate;
        set
        {
            Set(ref _noticeDate, value);
            RefreshCommands();
        }
    }

    public int PaymentTermDays
    {
        get => _paymentTermDays;
        set => Set(ref _paymentTermDays, value);
    }

    public decimal FeeAmount
    {
        get => _feeAmount;
        set => Set(ref _feeAmount, value);
    }

    public decimal AnnualInterestRatePercent
    {
        get => _annualInterestRatePercent;
        set => Set(ref _annualInterestRatePercent, value);
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
            var selectedNoticeId = SelectedNotice?.Id;
            var selectedInvoiceId = SelectedInvoice?.Id;

            Notices.Clear();
            OverdueInvoices.Clear();

            foreach (var notice in await _service.GetAllAsync())
                Notices.Add(notice);

            foreach (var invoice in await _invoices.GetAllAsync(
                         DateOnly.FromDateTime(DateTime.Today)))
            {
                if (invoice.IsOverdue && invoice.OpenAmount > 0m)
                    OverdueInvoices.Add(invoice);
            }

            SelectedNotice = selectedNoticeId.HasValue
                ? Notices.FirstOrDefault(x => x.Id == selectedNoticeId.Value)
                : Notices.FirstOrDefault();

            SelectedInvoice = selectedInvoiceId.HasValue
                ? OverdueInvoices.FirstOrDefault(x => x.Id == selectedInvoiceId.Value)
                : OverdueInvoices.FirstOrDefault();

            StatusText =
                $"{Notices.Count} Mahnbeleg(e), {OverdueInvoices.Count} überfällige Rechnung(en).";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnwesen konnte nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedInvoice is null || !NoticeDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateAsync(
                SelectedInvoice.Id,
                DateOnly.FromDateTime(NoticeDate.Value.DateTime),
                PaymentTermDays,
                FeeAmount,
                AnnualInterestRatePercent);

            await RefreshAsync();
            SelectedNotice = Notices.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Mahnentwurf {created.NoticeNumber} wurde erstellt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnung konnte nicht erstellt werden");
        }
    }

    private async Task IssueAsync()
    {
        if (SelectedNotice is null)
            return;

        try
        {
            await _service.IssueAsync(SelectedNotice.Id);
            await RefreshAsync();
            StatusText = "Mahnung wurde ausgestellt und die Mahnstufe aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnung konnte nicht ausgestellt werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedNotice is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedNotice.Id);
            await RefreshAsync();
            StatusText = "Mahnentwurf wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnung konnte nicht storniert werden");
        }
    }

    private async Task ExportPdfAsync()
    {
        if (SelectedNotice is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Belegarchiv",
                "Mahnungen");

            var result = await _service.ExportPdfAsync(
                SelectedNotice.Id,
                directory);

            Process.Start(new ProcessStartInfo(result.PdfPath)
            {
                UseShellExecute = true
            });

            StatusText =
                $"Mahnschreiben und SHA-256-Manifest wurden archiviert: {result.PdfPath}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnschreiben konnte nicht erstellt werden");
        }
    }

    private bool CanCreate() =>
        SelectedInvoice is not null &&
        NoticeDate.HasValue &&
        PaymentTermDays >= 0 &&
        FeeAmount >= 0m &&
        AnnualInterestRatePercent >= 0m;

    private bool HasNotice() => SelectedNotice is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateCommand,
            IssueCommand,
            CancelCommand,
            ExportPdfCommand
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
