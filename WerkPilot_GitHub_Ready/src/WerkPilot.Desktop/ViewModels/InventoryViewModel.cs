using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Inventory;
using WerkPilot.Application.Materials;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Inventory;

namespace WerkPilot.Desktop.ViewModels;

public sealed class InventoryViewModel : INotifyPropertyChanged
{
    private readonly InventoryService _inventory;
    private readonly MaterialService _materials;
    private readonly ProjectService _projects;
    private readonly ReorderSuggestionService _reorderSuggestions;
    private readonly InventoryValuationService _valuation;
    private InventoryItemDto? _selectedItem;
    private MaterialItemDto? _selectedMaterial;
    private InventoryMovementDto? _selectedMovement;
    private ProjectDto? _selectedProject;
    private string _storageLocation = string.Empty;
    private decimal _minimumStock;
    private InventoryMovementType _movementType = InventoryMovementType.Receipt;
    private decimal _movementQuantity;
    private string _movementReason = string.Empty;
    private string? _reference;
    private string _statusText = "Bereit";

    public InventoryViewModel(
        InventoryService inventory,
        MaterialService materials,
        ProjectService projects,
        ReorderSuggestionService reorderSuggestions,
        InventoryValuationService valuation)
    {
        _inventory = inventory;
        _materials = materials;
        _projects = projects;
        _reorderSuggestions = reorderSuggestions;
        _valuation = valuation;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateItemCommand = new AsyncCommand(CreateItemAsync, CanCreateItem);
        SaveMasterDataCommand = new AsyncCommand(SaveMasterDataAsync, HasSelectedItem);
        BookMovementCommand = new AsyncCommand(BookMovementAsync, CanBookMovement);
        RefreshReorderSuggestionsCommand = new AsyncCommand(RefreshReorderSuggestionsAsync);
        ExportReorderSuggestionsCommand = new AsyncCommand(ExportReorderSuggestionsAsync);
        RefreshValuationCommand = new AsyncCommand(RefreshValuationAsync);
        ExportValuationCommand = new AsyncCommand(ExportValuationAsync);
        _ = InitializeAsync();
    }

    public ObservableCollection<InventoryItemDto> Items { get; } = [];
    public ObservableCollection<InventoryMovementDto> Movements { get; } = [];
    public ObservableCollection<MaterialItemDto> Materials { get; } = [];
    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<ReorderSuggestionDto> ReorderSuggestions { get; } = [];
    public ObservableCollection<InventoryValuationItemDto> ValuationItems { get; } = [];
    public IReadOnlyList<InventoryMovementType> MovementTypes { get; } =
        Enum.GetValues<InventoryMovementType>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateItemCommand { get; }
    public ICommand SaveMasterDataCommand { get; }
    public ICommand BookMovementCommand { get; }
    public ICommand RefreshReorderSuggestionsCommand { get; }
    public ICommand ExportReorderSuggestionsCommand { get; }
    public ICommand RefreshValuationCommand { get; }
    public ICommand ExportValuationCommand { get; }

    public InventoryItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value))
            {
                StorageLocation = value?.StorageLocation ?? string.Empty;
                MinimumStock = value?.MinimumStock ?? 0m;
                _ = LoadMovementsAsync();
                RefreshCommands();
            }
        }
    }

    public MaterialItemDto? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            Set(ref _selectedMaterial, value);
            RefreshCommands();
        }
    }

    public InventoryMovementDto? SelectedMovement
    {
        get => _selectedMovement;
        set => Set(ref _selectedMovement, value);
    }

    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set => Set(ref _selectedProject, value);
    }

    public string StorageLocation { get => _storageLocation; set => Set(ref _storageLocation, value); }
    public decimal MinimumStock { get => _minimumStock; set => Set(ref _minimumStock, value); }
    public InventoryMovementType MovementType { get => _movementType; set => Set(ref _movementType, value); }
    public decimal MovementQuantity
    {
        get => _movementQuantity;
        set
        {
            Set(ref _movementQuantity, value);
            RefreshCommands();
        }
    }

    public string MovementReason
    {
        get => _movementReason;
        set
        {
            Set(ref _movementReason, value);
            RefreshCommands();
        }
    }

    public string? Reference { get => _reference; set => Set(ref _reference, value); }

    public decimal TotalStockValue { get; private set; }
    public decimal TotalReservedValue { get; private set; }
    public decimal TotalAvailableValue { get; private set; }
    public int OutdatedValuationPriceCount { get; private set; }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var material in await _materials.SearchAsync(null, includeInactive: false))
            Materials.Add(material);

        foreach (var project in await _projects.GetAllAsync())
            Projects.Add(project);

        SelectedMaterial = Materials.FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedItem?.Id;
            Items.Clear();

            foreach (var item in await _inventory.GetAllAsync())
                Items.Add(item);

            SelectedItem = selectedId.HasValue
                ? Items.FirstOrDefault(x => x.Id == selectedId.Value)
                : Items.FirstOrDefault();

            await RefreshReorderSuggestionsAsync();
            await RefreshValuationAsync();
            StatusText = $"{Items.Count} Lagerartikel geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Lagerbestand konnte nicht geladen werden");
        }
    }

    private async Task LoadMovementsAsync()
    {
        Movements.Clear();

        if (SelectedItem is null)
            return;

        foreach (var movement in await _inventory.GetMovementsAsync(SelectedItem.Id))
            Movements.Add(movement);
    }

    private async Task CreateItemAsync()
    {
        if (SelectedMaterial is null)
            return;

        try
        {
            await _inventory.CreateAsync(
                SelectedMaterial.Id,
                StorageLocation,
                MinimumStock);

            await RefreshAsync();
            StatusText = "Lagerartikel wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Lagerartikel konnte nicht angelegt werden");
        }
    }

    private async Task SaveMasterDataAsync()
    {
        if (SelectedItem is null)
            return;

        try
        {
            await _inventory.UpdateMasterDataAsync(
                SelectedItem.Id,
                StorageLocation,
                MinimumStock);

            await RefreshAsync();
            StatusText = "Lagerstammdaten wurden gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Lagerstammdaten konnten nicht gespeichert werden");
        }
    }

    private async Task BookMovementAsync()
    {
        if (SelectedItem is null)
            return;

        try
        {
            await _inventory.BookMovementAsync(
                SelectedItem.Id,
                MovementType,
                MovementQuantity,
                MovementReason,
                SelectedProject?.Id,
                Reference);

            MovementQuantity = 0m;
            MovementReason = string.Empty;
            Reference = null;

            await RefreshAsync();
            StatusText = "Lagerbewegung wurde gebucht.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Lagerbewegung konnte nicht gebucht werden");
        }
    }


    private async Task RefreshReorderSuggestionsAsync()
    {
        ReorderSuggestions.Clear();

        foreach (var suggestion in await _reorderSuggestions.GetAsync())
            ReorderSuggestions.Add(suggestion);
    }

    private async Task ExportReorderSuggestionsAsync()
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Nachbestellung");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"Nachbestellvorschlag_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _reorderSuggestions.ExportCsvAsync());

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = $"Nachbestellvorschlag wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Nachbestellvorschlag konnte nicht exportiert werden");
        }
    }


    private async Task RefreshValuationAsync()
    {
        var summary = await _valuation.GetAsync();

        ValuationItems.Clear();
        foreach (var item in summary.Items)
            ValuationItems.Add(item);

        TotalStockValue = summary.TotalStockValue;
        TotalReservedValue = summary.TotalReservedValue;
        TotalAvailableValue = summary.TotalAvailableValue;
        OutdatedValuationPriceCount = summary.OutdatedPriceCount;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalStockValue)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalReservedValue)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalAvailableValue)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutdatedValuationPriceCount)));
    }

    private async Task ExportValuationAsync()
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Lagerbewertung");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"Lagerbewertung_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _valuation.ExportCsvAsync());

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = $"Lagerbewertung wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Lagerbewertung konnte nicht exportiert werden");
        }
    }

    private bool CanCreateItem() => SelectedMaterial is not null;
    private bool HasSelectedItem() => SelectedItem is not null;

    private bool CanBookMovement() =>
        SelectedItem is not null &&
        MovementQuantity > 0 &&
        !string.IsNullOrWhiteSpace(MovementReason);

    private void RefreshCommands()
    {
        (CreateItemCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (SaveMasterDataCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (BookMovementCommand as AsyncCommand)?.RaiseCanExecuteChanged();
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
