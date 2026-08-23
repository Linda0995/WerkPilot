using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Offers;

namespace WerkPilot.Desktop.ViewModels;

public sealed class OffersViewModel : INotifyPropertyChanged
{
    private readonly OfferService _offers;
    private readonly CustomerService _customers;
    private readonly AuthorizationService _authorization;
    private readonly OfferDocumentService _documents;
    private readonly OfferEmailService _emailService;
    private OfferDto? _selectedOffer;
    private CustomerDto? _selectedCustomer;
    private string _newTitle = string.Empty;
    private DateTimeOffset? _validUntil = DateTimeOffset.Now.AddDays(30);
    private string _positionDescription = string.Empty;
    private decimal _positionQuantity = 1m;
    private decimal _positionUnitPrice;
    private bool _positionIsOptional;
    private decimal _discountPercent;
    private OfferPositionDto? _selectedPosition;
    private string _statusText = "Bereit";
    private decimal _netTotal;
    private decimal _taxTotal;
    private decimal _grossTotal;
    private string _emailRecipient = string.Empty;
    private string _emailSubject = string.Empty;
    private string _emailBody = string.Empty;

    public OffersViewModel(
        OfferService offers,
        CustomerService customers,
        AuthorizationService authorization,
        OfferDocumentService documents,
        OfferEmailService emailService)
    {
        _offers = offers;
        _customers = customers;
        _authorization = authorization;
        _documents = documents;
        _emailService = emailService;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateOfferCommand = new AsyncCommand(CreateOfferAsync, CanCreate);
        AddPositionCommand = new AsyncCommand(AddPositionAsync, CanEditSelected);
        MarkSentCommand = new AsyncCommand(MarkSentAsync, CanEditSelected);
        AcceptCommand = new AsyncCommand(AcceptAsync, CanEditSelected);
        RejectCommand = new AsyncCommand(RejectAsync, CanEditSelected);
        UpdatePositionCommand = new AsyncCommand(UpdatePositionAsync, CanEditPosition);
        RemovePositionCommand = new AsyncCommand(RemovePositionAsync, CanEditPosition);
        ApplyDiscountCommand = new AsyncCommand(ApplyDiscountAsync, CanEditSelected);
        DuplicateOfferCommand = new AsyncCommand(DuplicateOfferAsync, HasSelectedOffer);
        ExportPdfCommand = new AsyncCommand(ExportPdfAsync, HasSelectedOffer);
        PreviewPdfCommand = new AsyncCommand(PreviewPdfAsync, HasSelectedOffer);
        LoadEmailPreviewCommand = new AsyncCommand(LoadEmailPreviewAsync, HasSelectedOffer);
        SendEmailCommand = new AsyncCommand(SendEmailAsync, CanSendEmail);

        _ = InitializeAsync();
    }

    public ObservableCollection<OfferDto> Offers { get; } = [];
    public ObservableCollection<CustomerDto> Customers { get; } = [];
    public ObservableCollection<OfferPositionDto> Positions { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateOfferCommand { get; }
    public ICommand AddPositionCommand { get; }
    public ICommand MarkSentCommand { get; }
    public ICommand AcceptCommand { get; }
    public ICommand RejectCommand { get; }
    public ICommand UpdatePositionCommand { get; }
    public ICommand RemovePositionCommand { get; }
    public ICommand ApplyDiscountCommand { get; }
    public ICommand DuplicateOfferCommand { get; }
    public ICommand ExportPdfCommand { get; }
    public ICommand PreviewPdfCommand { get; }
    public ICommand LoadEmailPreviewCommand { get; }
    public ICommand SendEmailCommand { get; }

    public bool CanEditOffers => _authorization.CanEditOffers();

    public OfferDto? SelectedOffer
    {
        get => _selectedOffer;
        set
        {
            if (Set(ref _selectedOffer, value))
            {
                _ = LoadSelectedAsync();
                RefreshCommands();
            }
        }
    }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            Set(ref _selectedCustomer, value);
            RefreshCommands();
        }
    }

    public string NewTitle { get => _newTitle; set { Set(ref _newTitle, value); RefreshCommands(); } }
    public DateTimeOffset? ValidUntil { get => _validUntil; set => Set(ref _validUntil, value); }
    public OfferPositionDto? SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            if (Set(ref _selectedPosition, value) && value is not null)
            {
                PositionDescription = value.Description;
                PositionQuantity = value.Quantity;
                PositionUnitPrice = value.UnitPriceNet;
                PositionIsOptional = value.IsOptional;
            }

            RefreshCommands();
        }
    }

    public string PositionDescription { get => _positionDescription; set => Set(ref _positionDescription, value); }
    public decimal PositionQuantity { get => _positionQuantity; set => Set(ref _positionQuantity, value); }
    public decimal PositionUnitPrice { get => _positionUnitPrice; set => Set(ref _positionUnitPrice, value); }
    public bool PositionIsOptional { get => _positionIsOptional; set => Set(ref _positionIsOptional, value); }
    public decimal DiscountPercent { get => _discountPercent; set => Set(ref _discountPercent, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }
    public decimal NetTotal { get => _netTotal; private set => Set(ref _netTotal, value); }
    public decimal TaxTotal { get => _taxTotal; private set => Set(ref _taxTotal, value); }
    public decimal GrossTotal { get => _grossTotal; private set => Set(ref _grossTotal, value); }
    public string EmailRecipient { get => _emailRecipient; set { Set(ref _emailRecipient, value); RefreshCommands(); } }
    public string EmailSubject { get => _emailSubject; set => Set(ref _emailSubject, value); }
    public string EmailBody { get => _emailBody; set => Set(ref _emailBody, value); }

    private async Task InitializeAsync()
    {
        var customers = await _customers.SearchAsync(null);
        Customers.Clear();
        foreach (var customer in customers)
            Customers.Add(customer);

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            await _offers.MarkExpiredAsync(DateOnly.FromDateTime(DateTime.Today));
            var selectedId = SelectedOffer?.Id;
            var offers = await _offers.GetAllAsync();
            Offers.Clear();
            foreach (var offer in offers)
                Offers.Add(offer);

            SelectedOffer = selectedId is null
                ? Offers.FirstOrDefault()
                : Offers.FirstOrDefault(x => x.Id == selectedId);

            StatusText = $"{Offers.Count} Angebot(e) geladen";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Fehler beim Laden");
        }
    }

    private async Task LoadSelectedAsync()
    {
        Positions.Clear();
        NetTotal = TaxTotal = GrossTotal = 0m;

        if (SelectedOffer is null)
            return;

        try
        {
            var details = await _offers.GetAsync(SelectedOffer.Id);
            foreach (var position in details.Positions)
                Positions.Add(position);

            SelectedPosition = null;

            NetTotal = details.NetTotal;
            TaxTotal = details.TaxTotal;
            GrossTotal = details.GrossTotal;
            DiscountPercent = details.DiscountPercent;
            await LoadEmailPreviewAsync();
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Angebot konnte nicht geladen werden");
        }
    }

    private async Task CreateOfferAsync()
    {
        try
        {
            if (SelectedCustomer is null)
                return;

            var validUntil = DateOnly.FromDateTime((ValidUntil ?? DateTimeOffset.Now.AddDays(30)).DateTime);
            var created = await _offers.CreateAsync(
                SelectedCustomer.Id,
                NewTitle,
                validUntil);

            NewTitle = string.Empty;
            await RefreshAsync();
            SelectedOffer = Offers.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Angebot {created.OfferNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Angebot konnte nicht angelegt werden");
        }
    }

    private async Task AddPositionAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            await _offers.AddPositionAsync(
                SelectedOffer.Id,
                PositionDescription,
                PositionQuantity,
                PositionUnitPrice,
                PositionIsOptional);

            PositionDescription = string.Empty;
            PositionQuantity = 1m;
            PositionUnitPrice = 0m;
            PositionIsOptional = false;
            await RefreshAsync();
            StatusText = "Position wurde hinzugefügt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht hinzugefügt werden");
        }
    }

    private async Task UpdatePositionAsync()
    {
        if (SelectedOffer is null || SelectedPosition is null)
            return;

        try
        {
            await _offers.UpdatePositionAsync(new UpdateOfferPositionRequest(
                SelectedOffer.Id,
                SelectedPosition.Id,
                PositionDescription,
                PositionQuantity,
                PositionUnitPrice,
                PositionIsOptional));

            await RefreshAsync();
            StatusText = "Position wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht aktualisiert werden");
        }
    }

    private async Task ApplyDiscountAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            await _offers.SetDiscountAsync(SelectedOffer.Id, DiscountPercent);
            await RefreshAsync();
            StatusText = "Angebotsrabatt wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Rabatt konnte nicht gespeichert werden");
        }
    }

    private async Task RemovePositionAsync()
    {
        if (SelectedOffer is null || SelectedPosition is null)
            return;

        try
        {
            await _offers.RemovePositionAsync(SelectedOffer.Id, SelectedPosition.Id);
            await RefreshAsync();
            StatusText = "Position wurde entfernt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht entfernt werden");
        }
    }

    private async Task DuplicateOfferAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var copy = await _offers.DuplicateAsync(
                SelectedOffer.Id,
                DateOnly.FromDateTime(DateTime.Today.AddDays(30)));

            await RefreshAsync();
            SelectedOffer = Offers.FirstOrDefault(x => x.Id == copy.Id);
            StatusText = $"Angebot wurde als {copy.OfferNumber} dupliziert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Angebot konnte nicht dupliziert werden");
        }
    }

    private async Task MarkSentAsync()
    {
        if (SelectedOffer is null) return;
        try
        {
            await _offers.MarkSentAsync(SelectedOffer.Id);
            await RefreshAsync();
            StatusText = "Angebot wurde als gesendet markiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorgang fehlgeschlagen");
        }
    }

    private async Task AcceptAsync()
    {
        if (SelectedOffer is null) return;
        try
        {
            await _offers.AcceptAsync(SelectedOffer.Id);
            await RefreshAsync();
            StatusText = "Angebot wurde angenommen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorgang fehlgeschlagen");
        }
    }

    private async Task LoadEmailPreviewAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var preview = await _emailService.CreatePreviewAsync(SelectedOffer.Id);
            EmailRecipient = preview.Recipient;
            EmailSubject = preview.Subject;
            EmailBody = preview.Body;
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "E-Mail-Vorlage konnte nicht geladen werden");
        }
    }

    private async Task SendEmailAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            await _emailService.SendAsync(new SendOfferEmailRequest(
                SelectedOffer.Id,
                EmailRecipient,
                EmailSubject,
                EmailBody));

            await RefreshAsync();
            StatusText = $"Angebot wurde an {EmailRecipient} versendet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "E-Mail-Versand fehlgeschlagen");
        }
    }

    private async Task ExportPdfAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var path = await _documents.ExportPdfAsync(
                SelectedOffer.Id,
                GetExportDirectory());

            StatusText = $"PDF wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "PDF konnte nicht erstellt werden");
        }
    }

    private async Task PreviewPdfAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var path = await _documents.ExportPdfAsync(
                SelectedOffer.Id,
                GetExportDirectory());

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = "PDF-Vorschau wurde im Standardprogramm geöffnet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "PDF-Vorschau konnte nicht geöffnet werden");
        }
    }

    private static string GetExportDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "WerkPilot",
            "Exporte",
            "Angebote");

    private async Task RejectAsync()
    {
        if (SelectedOffer is null) return;
        try
        {
            await _offers.RejectAsync(SelectedOffer.Id);
            await RefreshAsync();
            StatusText = "Angebot wurde abgelehnt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorgang fehlgeschlagen");
        }
    }

    private bool HasSelectedOffer() => SelectedOffer is not null;
    private bool CanEditPosition() =>
        CanEditOffers &&
        SelectedOffer is not null &&
        SelectedPosition is not null &&
        SelectedOffer.Status == WerkPilot.Domain.Offers.OfferStatus.Draft;

    private bool CanSendEmail() =>
        CanEditOffers &&
        SelectedOffer is not null &&
        !string.IsNullOrWhiteSpace(EmailRecipient);

    private bool CanCreate() =>
        CanEditOffers && SelectedCustomer is not null && !string.IsNullOrWhiteSpace(NewTitle);

    private bool CanEditSelected() =>
        CanEditOffers && SelectedOffer is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateOfferCommand, AddPositionCommand, MarkSentCommand, AcceptCommand, RejectCommand,
            ExportPdfCommand, PreviewPdfCommand, LoadEmailPreviewCommand, SendEmailCommand,
            UpdatePositionCommand, RemovePositionCommand, DuplicateOfferCommand,
            ApplyDiscountCommand
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
