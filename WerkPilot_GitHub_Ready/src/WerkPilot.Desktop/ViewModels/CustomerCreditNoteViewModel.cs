using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Billing;
using WerkPilot.Domain.Billing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CustomerCreditNoteViewModel : INotifyPropertyChanged
{
    private readonly CustomerCreditNoteService _creditNotes;
    private readonly CustomerInvoiceService _invoices;
    private CustomerCreditNoteDto? _selectedCreditNote;
    private CustomerInvoiceDto? _selectedInvoice;
    private CustomerInvoiceLineDto? _selectedInvoiceLine;
    private DateTimeOffset? _creditNoteDate = DateTimeOffset.Now;
    private string _reason = string.Empty;
    private decimal _quantity = 1m;
    private string _statusText = "Bereit";

    public CustomerCreditNoteViewModel(
        CustomerCreditNoteService creditNotes,
        CustomerInvoiceService invoices)
    {
        _creditNotes = creditNotes;
        _invoices = invoices;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateFullCommand = new AsyncCommand(CreateFullAsync, CanCreate);
        CreatePartialCommand = new AsyncCommand(CreatePartialAsync, CanCreatePartial);
        IssueCommand = new AsyncCommand(IssueAsync, HasCreditNote);
        ApplyCommand = new AsyncCommand(ApplyAsync, HasCreditNote);
        CancelCommand = new AsyncCommand(CancelAsync, HasCreditNote);
        ExportCommand = new AsyncCommand(ExportAsync, HasCreditNote);
        ExportPdfCommand = new AsyncCommand(ExportPdfAsync, HasCreditNote);

        _ = RefreshAsync();
    }

    public ObservableCollection<CustomerCreditNoteDto> CreditNotes { get; } = [];
    public ObservableCollection<CustomerCreditNoteLineDto> CreditNoteLines { get; } = [];
    public ObservableCollection<CustomerInvoiceDto> Invoices { get; } = [];
    public ObservableCollection<CustomerInvoiceLineDto> InvoiceLines { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateFullCommand { get; }
    public ICommand CreatePartialCommand { get; }
    public ICommand IssueCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public CustomerCreditNoteDto? SelectedCreditNote
    {
        get => _selectedCreditNote;
        set
        {
            if (Set(ref _selectedCreditNote, value))
            {
                CreditNoteLines.Clear();
                if (value is not null)
                    foreach (var line in value.Lines)
                        CreditNoteLines.Add(line);

                RefreshCommands();
            }
        }
    }

    public CustomerInvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (Set(ref _selectedInvoice, value))
            {
                InvoiceLines.Clear();
                if (value is not null)
                    foreach (var line in value.Lines)
                        InvoiceLines.Add(line);

                SelectedInvoiceLine = InvoiceLines.FirstOrDefault();
                RefreshCommands();
            }
        }
    }

    public CustomerInvoiceLineDto? SelectedInvoiceLine
    {
        get => _selectedInvoiceLine;
        set
        {
            if (Set(ref _selectedInvoiceLine, value))
            {
                Quantity = value?.Quantity ?? 1m;
                RefreshCommands();
            }
        }
    }

    public DateTimeOffset? CreditNoteDate
    {
        get => _creditNoteDate;
        set { Set(ref _creditNoteDate, value); RefreshCommands(); }
    }

    public string Reason
    {
        get => _reason;
        set { Set(ref _reason, value); RefreshCommands(); }
    }

    public decimal Quantity
    {
        get => _quantity;
        set { Set(ref _quantity, value); RefreshCommands(); }
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
            var selectedCreditId = SelectedCreditNote?.Id;
            var selectedInvoiceId = SelectedInvoice?.Id;

            CreditNotes.Clear();
            Invoices.Clear();

            foreach (var note in await _creditNotes.GetAllAsync())
                CreditNotes.Add(note);

            foreach (var invoice in await _invoices.GetAllAsync(
                         DateOnly.FromDateTime(DateTime.Today)))
            {
                if (invoice.Status is not CustomerInvoiceStatus.Draft
                    and not CustomerInvoiceStatus.Cancelled &&
                    invoice.OpenAmount > 0m)
                {
                    Invoices.Add(invoice);
                }
            }

            SelectedCreditNote = selectedCreditId.HasValue
                ? CreditNotes.FirstOrDefault(x => x.Id == selectedCreditId.Value)
                : CreditNotes.FirstOrDefault();

            SelectedInvoice = selectedInvoiceId.HasValue
                ? Invoices.FirstOrDefault(x => x.Id == selectedInvoiceId.Value)
                : Invoices.FirstOrDefault();

            StatusText = $"{CreditNotes.Count} Gutschrift(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Gutschriften konnten nicht geladen werden");
        }
    }

    private async Task CreateFullAsync()
    {
        if (SelectedInvoice is null || !CreditNoteDate.HasValue)
            return;

        try
        {
            var created = await _creditNotes.CreateFullAsync(
                SelectedInvoice.Id,
                DateOnly.FromDateTime(CreditNoteDate.Value.DateTime),
                Reason);

            await RefreshAsync();
            SelectedCreditNote = CreditNotes.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Vollgutschrift {created.CreditNoteNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vollgutschrift konnte nicht angelegt werden");
        }
    }

    private async Task CreatePartialAsync()
    {
        if (SelectedInvoice is null ||
            SelectedInvoiceLine is null ||
            !CreditNoteDate.HasValue)
            return;

        try
        {
            var created = await _creditNotes.CreatePartialAsync(
                SelectedInvoice.Id,
                SelectedInvoiceLine.Id,
                Quantity,
                DateOnly.FromDateTime(CreditNoteDate.Value.DateTime),
                Reason);

            await RefreshAsync();
            SelectedCreditNote = CreditNotes.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Teilgutschrift {created.CreditNoteNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Teilgutschrift konnte nicht angelegt werden");
        }
    }

    private async Task IssueAsync()
    {
        if (SelectedCreditNote is null)
            return;

        try
        {
            await _creditNotes.IssueAsync(SelectedCreditNote.Id);
            await RefreshAsync();
            StatusText = "Gutschrift wurde ausgestellt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Gutschrift konnte nicht ausgestellt werden");
        }
    }

    private async Task ApplyAsync()
    {
        if (SelectedCreditNote is null)
            return;

        try
        {
            await _creditNotes.ApplyAsync(SelectedCreditNote.Id);
            await RefreshAsync();
            StatusText = "Gutschrift wurde mit der Rechnung verrechnet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Gutschrift konnte nicht verrechnet werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedCreditNote is null)
            return;

        try
        {
            await _creditNotes.CancelAsync(SelectedCreditNote.Id);
            await RefreshAsync();
            StatusText = "Gutschrift wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Gutschrift konnte nicht storniert werden");
        }
    }

    private async Task ExportAsync()
    {
        if (SelectedCreditNote is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Gutschriften");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedCreditNote.CreditNoteNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _creditNotes.ExportCsvAsync(SelectedCreditNote.Id));

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"Gutschrift wurde exportiert: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Gutschrift konnte nicht exportiert werden");
        }
    }


    private async Task ExportPdfAsync()
    {
        if (SelectedCreditNote is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Belegarchiv",
                "Gutschriften");

            var result = await _creditNotes.ExportPdfAsync(
                SelectedCreditNote.Id,
                directory);

            Process.Start(new ProcessStartInfo(result.PdfPath)
            {
                UseShellExecute = true
            });

            StatusText = $"PDF-Beleg und Prüfsummen-Manifest wurden archiviert: {result.PdfPath}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "PDF-Beleg konnte nicht erstellt werden");
        }
    }

    private bool CanCreate() =>
        SelectedInvoice is not null &&
        CreditNoteDate.HasValue &&
        !string.IsNullOrWhiteSpace(Reason);

    private bool CanCreatePartial() =>
        CanCreate() &&
        SelectedInvoiceLine is not null &&
        Quantity > 0m &&
        Quantity <= SelectedInvoiceLine.Quantity;

    private bool HasCreditNote() => SelectedCreditNote is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateFullCommand,
            CreatePartialCommand,
            IssueCommand,
            ApplyCommand,
            CancelCommand,
            ExportCommand,
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
