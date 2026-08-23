using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Settings;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CompanySettingsViewModel : INotifyPropertyChanged
{
    private readonly CompanyProfileService _service;
    private string _companyName = string.Empty;
    private string? _street;
    private string? _postalCode;
    private string? _city;
    private string _countryCode = "AT";
    private string? _email;
    private string? _phone;
    private string? _vatId;
    private string? _website;
    private string _offerIntroText = string.Empty;
    private string _offerClosingText = string.Empty;
    private string _currencyCode = "EUR";
    private string _offerEmailSubjectTemplate = string.Empty;
    private string _offerEmailBodyTemplate = string.Empty;
    private string _statusText = "Bereit";

    public CompanySettingsViewModel(CompanyProfileService service)
    {
        _service = service;
        SaveCommand = new AsyncCommand(SaveAsync);
        _ = LoadAsync();
    }

    public ICommand SaveCommand { get; }
    public string CompanyName { get => _companyName; set => Set(ref _companyName, value); }
    public string? Street { get => _street; set => Set(ref _street, value); }
    public string? PostalCode { get => _postalCode; set => Set(ref _postalCode, value); }
    public string? City { get => _city; set => Set(ref _city, value); }
    public string CountryCode { get => _countryCode; set => Set(ref _countryCode, value); }
    public string? Email { get => _email; set => Set(ref _email, value); }
    public string? Phone { get => _phone; set => Set(ref _phone, value); }
    public string? VatId { get => _vatId; set => Set(ref _vatId, value); }
    public string? Website { get => _website; set => Set(ref _website, value); }
    public string OfferIntroText { get => _offerIntroText; set => Set(ref _offerIntroText, value); }
    public string OfferClosingText { get => _offerClosingText; set => Set(ref _offerClosingText, value); }
    public string CurrencyCode { get => _currencyCode; set => Set(ref _currencyCode, value); }
    public string OfferEmailSubjectTemplate { get => _offerEmailSubjectTemplate; set => Set(ref _offerEmailSubjectTemplate, value); }
    public string OfferEmailBodyTemplate { get => _offerEmailBodyTemplate; set => Set(ref _offerEmailBodyTemplate, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task LoadAsync()
    {
        try
        {
            var profile = await _service.GetAsync();
            CompanyName = profile.CompanyName;
            Street = profile.Street;
            PostalCode = profile.PostalCode;
            City = profile.City;
            CountryCode = profile.CountryCode;
            Email = profile.Email;
            Phone = profile.Phone;
            VatId = profile.VatId;
            Website = profile.Website;
            OfferIntroText = profile.OfferIntroText;
            OfferClosingText = profile.OfferClosingText;
            CurrencyCode = profile.CurrencyCode;
            OfferEmailSubjectTemplate = profile.OfferEmailSubjectTemplate;
            OfferEmailBodyTemplate = profile.OfferEmailBodyTemplate;
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Einstellungen konnten nicht geladen werden");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            await _service.UpdateAsync(new UpdateCompanyProfileRequest(
                CompanyName, Street, PostalCode, City, CountryCode,
                Email, Phone, VatId, Website,
                OfferIntroText, OfferClosingText, CurrencyCode,
                OfferEmailSubjectTemplate, OfferEmailBodyTemplate));

            StatusText = "Firmenstammdaten und Angebotsvorlage wurden gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Speichern fehlgeschlagen");
        }
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

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running;
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (_running) return;
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
