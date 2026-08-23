using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Materials;

namespace WerkPilot.Desktop.ViewModels;

public sealed class MaterialViewModel : INotifyPropertyChanged
{
    private readonly MaterialService _service;
    private MaterialItemDto? _selectedItem;
    private string _searchText = string.Empty;
    private bool _includeInactive;
    private string _articleNumber = string.Empty;
    private string _description = string.Empty;
    private string _unit = "Stk";
    private decimal _purchasePrice;
    private string? _supplier;
    private string? _supplierArticleNumber;
    private string _statusText = "Bereit";

    public MaterialViewModel(MaterialService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync);
        UpdateCommand = new AsyncCommand(UpdateAsync, () => SelectedItem is not null);
        ToggleActiveCommand = new AsyncCommand(ToggleActiveAsync, () => SelectedItem is not null);
        ImportCsvCommand = new AsyncCommand(ImportCsvAsync);
        ExportCsvCommand = new AsyncCommand(ExportCsvAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<MaterialItemDto> Items { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand ToggleActiveCommand { get; }
    public ICommand ImportCsvCommand { get; }
    public ICommand ExportCsvCommand { get; }

    public MaterialItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value) && value is not null)
            {
                ArticleNumber = value.ArticleNumber;
                Description = value.Description;
                Unit = value.Unit;
                PurchasePrice = value.PurchasePrice;
                Supplier = value.Supplier;
                SupplierArticleNumber = value.SupplierArticleNumber;
            }

            RefreshCommands();
        }
    }

    public string SearchText { get => _searchText; set => Set(ref _searchText, value); }
    public bool IncludeInactive
    {
        get => _includeInactive;
        set
        {
            if (Set(ref _includeInactive, value))
                _ = RefreshAsync();
        }
    }

    public string ArticleNumber { get => _articleNumber; set => Set(ref _articleNumber, value); }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string Unit { get => _unit; set => Set(ref _unit, value); }
    public decimal PurchasePrice { get => _purchasePrice; set => Set(ref _purchasePrice, value); }
    public string? Supplier { get => _supplier; set => Set(ref _supplier, value); }
    public string? SupplierArticleNumber { get => _supplierArticleNumber; set => Set(ref _supplierArticleNumber, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task ImportCsvAsync()
    {
        try
        {
            var path = Path.Combine(GetExchangeDirectory(), "material_import.csv");

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(GetExchangeDirectory());
                await File.WriteAllTextAsync(
                    path,
                    "Artikelnummer;Beschreibung;Einheit;Einkaufspreis;Lieferant;LieferantenArtikelnummer\n"
                    + "MAT-001;Beispielartikel;Stk;10,50;Beispiellieferant;L-001\n");

                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                StatusText = $"Importvorlage wurde erstellt: {path}";
                return;
            }

            var csv = await File.ReadAllTextAsync(path);
            var result = await _service.ImportCsvAsync(csv);
            await RefreshAsync();

            StatusText =
                $"Import abgeschlossen: {result.CreatedCount} neu, "
                + $"{result.UpdatedCount} aktualisiert, {result.SkippedCount} übersprungen."
                + (result.Errors.Count > 0 ? $" Erster Fehler: {result.Errors[0]}" : string.Empty);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CSV-Import fehlgeschlagen");
        }
    }

    private async Task ExportCsvAsync()
    {
        try
        {
            Directory.CreateDirectory(GetExchangeDirectory());
            var path = Path.Combine(
                GetExchangeDirectory(),
                $"material_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            var csv = await _service.ExportCsvAsync(includeInactive: true);
            await File.WriteAllTextAsync(path, csv);

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"Materialexport wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CSV-Export fehlgeschlagen");
        }
    }

    private static string GetExchangeDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "WerkPilot",
            "ImportExport");

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedItem?.Id;
            var items = await _service.SearchAsync(SearchText, IncludeInactive);

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);

            SelectedItem = selectedId is null
                ? null
                : Items.FirstOrDefault(x => x.Id == selectedId);

            StatusText = $"{Items.Count} Materialartikel geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Materialstamm konnte nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        try
        {
            var created = await _service.CreateAsync(
                ArticleNumber,
                Description,
                Unit,
                PurchasePrice,
                Supplier,
                SupplierArticleNumber);

            await RefreshAsync();
            SelectedItem = Items.FirstOrDefault(x => x.Id == created.Id);
            StatusText = "Materialartikel wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Materialartikel konnte nicht angelegt werden");
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedItem is null)
            return;

        try
        {
            await _service.UpdateAsync(
                SelectedItem.Id,
                ArticleNumber,
                Description,
                Unit,
                PurchasePrice,
                Supplier,
                SupplierArticleNumber);

            await RefreshAsync();
            StatusText = "Materialartikel wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Materialartikel konnte nicht aktualisiert werden");
        }
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedItem is null)
            return;

        await _service.SetActiveAsync(SelectedItem.Id, !SelectedItem.IsActive);
        await RefreshAsync();
        StatusText = SelectedItem.IsActive
            ? "Materialartikel wurde deaktiviert."
            : "Materialartikel wurde aktiviert.";
    }

    private void RefreshCommands()
    {
        foreach (var command in new[] { UpdateCommand, ToggleActiveCommand })
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
