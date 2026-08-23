using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Billing;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CustomerInvoiceViewModel : INotifyPropertyChanged
{
    private readonly CustomerInvoiceService _service;
    private readonly OfferService _offers;
    private readonly ProjectService _projects;
    private CustomerInvoiceDto? _selectedInvoice;
    private OfferDto? _selectedOffer;
    private ProjectDto? _selectedProject;
    private DateTimeOffset? _invoiceDate = DateTimeOffset.Now;
    private DateTimeOffset? _dueDate = DateTimeOffset.Now.AddDays(14);
    private decimal _vatRatePercent = 20m;
    private decimal _paymentAmount;
    private DateTimeOffset? _paymentDate = DateTimeOffset.Now;
    private string? _paymentReference;
    private string _statusText = "Bereit";

    public CustomerInvoiceViewModel(
        CustomerInvoiceService service,
        OfferService offers,
        ProjectService projects)
    {
        _service = service;
        _offers = offers;
        _projects = projects;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateFromOfferCommand = new AsyncCommand(CreateFromOfferAsync, CanCreateFromOffer);
        CreateFromProjectCommand = new AsyncCommand(CreateFromProjectAsync, CanCreateFromProject);
        IssueCommand = new AsyncCommand(IssueAsync, HasInvoice);
        RegisterPaymentCommand = new AsyncCommand(RegisterPaymentAsync, CanRegisterPayment);
        AdvanceDunningCommand = new AsyncCommand(AdvanceDunningAsync, HasInvoice);
        CancelCommand = new AsyncCommand(CancelAsync, HasInvoice);
        ExportCommand = new AsyncCommand(ExportAsync, HasInvoice);
        ExportPdfCommand = new AsyncCommand(ExportPdfAsync, HasInvoice);

        _ = InitializeAsync();
    }

    public ObservableCollection<CustomerInvoiceDto> Invoices { get; } = [];
    public ObservableCollection<CustomerInvoiceLineDto> Lines { get; } = [];
    public ObservableCollection<CustomerInvoicePaymentDto> Payments { get; } = [];
    public ObservableCollection<OfferDto> Offers { get; } = [];
    public ObservableCollection<ProjectDto> Projects { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateFromOfferCommand { get; }
    public ICommand CreateFromProjectCommand { get; }
    public ICommand IssueCommand { get; }
    public ICommand RegisterPaymentCommand { get; }
    public ICommand AdvanceDunningCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public CustomerInvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (Set(ref _selectedInvoice, value))
            {
                Lines.Clear();
                Payments.Clear();

                if (value is not null)
                {
                    foreach (var line in value.Lines)
                        Lines.Add(line);
                    foreach (var payment in value.Payments)
                        Payments.Add(payment);
                }

                PaymentAmount = value?.OpenAmount ?? 0m;
                RefreshCommands();
            }
        }
    }

    public OfferDto? SelectedOffer
    {
        get => _selectedOffer;
        set
        {
            Set(ref _selectedOffer, value);
            RefreshCommands();
        }
    }

    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set
        {
            Set(ref _selectedProject, value);
            RefreshCommands();
        }
    }

    public DateTimeOffset? InvoiceDate { get => _invoiceDate; set { Set(ref _invoiceDate, value); RefreshCommands(); } }
    public DateTimeOffset? DueDate { get => _dueDate; set { Set(ref _dueDate, value); RefreshCommands(); } }
    public decimal VatRatePercent { get => _vatRatePercent; set => Set(ref _vatRatePercent, value); }
    public decimal PaymentAmount { get => _paymentAmount; set { Set(ref _paymentAmount, value); RefreshCommands(); } }
    public DateTimeOffset? PaymentDate { get => _paymentDate; set { Set(ref _paymentDate, value); RefreshCommands(); } }
    public string? PaymentReference { get => _paymentReference; set => Set(ref _paymentReference, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var offer in await _offers.GetAllAsync())
            Offers.Add(offer);

        foreach (var project in await _projects.GetAllAsync())
            Projects.Add(project);

        SelectedOffer = Offers.FirstOrDefault();
        SelectedProject = Projects.FirstOrDefault();

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedInvoice?.Id;
            Invoices.Clear();

            foreach (var invoice in await _service.GetAllAsync(
                         DateOnly.FromDateTime(DateTime.Today)))
                Invoices.Add(invoice);

            SelectedInvoice = selectedId.HasValue
                ? Invoices.FirstOrDefault(x => x.Id == selectedId.Value)
                : Invoices.FirstOrDefault();

            StatusText = $"{Invoices.Count} Ausgangsrechnung(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ausgangsrechnungen konnten nicht geladen werden");
        }
    }

    private async Task CreateFromOfferAsync()
    {
        if (SelectedOffer is null || !InvoiceDate.HasValue || !DueDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateFromOfferAsync(
                SelectedOffer.Id,
                DateOnly.FromDateTime(InvoiceDate.Value.DateTime),
                DateOnly.FromDateTime(DueDate.Value.DateTime),
                VatRatePercent);

            await RefreshAsync();
            SelectedInvoice = Invoices.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Rechnung {created.InvoiceNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht angelegt werden");
        }
    }

    private async Task CreateFromProjectAsync()
    {
        if (SelectedProject is null || !InvoiceDate.HasValue || !DueDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateFromProjectAsync(
                SelectedProject.Id,
                DateOnly.FromDateTime(InvoiceDate.Value.DateTime),
                DateOnly.FromDateTime(DueDate.Value.DateTime),
                VatRatePercent);

            await RefreshAsync();
            SelectedInvoice = Invoices.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Rechnung {created.InvoiceNumber} wurde aus Projekt erzeugt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projekt-Rechnung konnte nicht angelegt werden");
        }
    }

    private async Task IssueAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.IssueAsync(SelectedInvoice.Id);
            await RefreshAsync();
            StatusText = "Rechnung wurde ausgestellt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht ausgestellt werden");
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
            StatusText = "Kundenzahlung wurde erfasst.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Kundenzahlung konnte nicht erfasst werden");
        }
    }

    private async Task AdvanceDunningAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            await _service.AdvanceDunningAsync(
                SelectedInvoice.Id,
                DateOnly.FromDateTime(DateTime.Today));

            await RefreshAsync();
            StatusText = "Mahnstufe wurde erhöht.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mahnstufe konnte nicht erhöht werden");
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
                "Ausgangsrechnungen");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _service.ExportCsvAsync(SelectedInvoice.Id));

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"Rechnung wurde exportiert: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rechnung konnte nicht exportiert werden");
        }
    }


    private async Task ExportPdfAsync()
    {
        if (SelectedInvoice is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Belegarchiv",
                "Ausgangsrechnungen");

            var result = await _service.ExportPdfAsync(
                SelectedInvoice.Id,
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

    private bool CanCreateFromOffer() =>
        SelectedOffer is not null && InvoiceDate.HasValue && DueDate.HasValue;

    private bool CanCreateFromProject() =>
        SelectedProject is not null && InvoiceDate.HasValue && DueDate.HasValue;

    private bool HasInvoice() => SelectedInvoice is not null;

    private bool CanRegisterPayment() =>
        SelectedInvoice is not null &&
        PaymentDate.HasValue &&
        PaymentAmount > 0m &&
        PaymentAmount <= SelectedInvoice.OpenAmount;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateFromOfferCommand,
            CreateFromProjectCommand,
            IssueCommand,
            RegisterPaymentCommand,
            AdvanceDunningCommand,
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
