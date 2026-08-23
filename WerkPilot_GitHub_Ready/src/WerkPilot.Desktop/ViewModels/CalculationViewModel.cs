using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Calculation;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Materials;
using WerkPilot.Domain.Calculation;

namespace WerkPilot.Desktop.ViewModels;

public sealed class CalculationViewModel : INotifyPropertyChanged
{
    private readonly CalculationService _calculations;
    private readonly OfferService _offers;
    private readonly AuthorizationService _authorization;
    private readonly MaterialService _materials;
    private readonly PurchaseListService _purchaseListService;
    private OfferDto? _selectedOffer;
    private MaterialItemDto? _selectedMaterial;
    private decimal _materialQuantity = 1m;
    private CalculationItemDto? _selectedItem;
    private CostType _newCostType = CostType.Material;
    private string _newDescription = string.Empty;
    private decimal _newQuantity = 1m;
    private decimal _newUnitCost;
    private decimal _profitTargetPercent = 20m;
    private decimal _materialCost;
    private decimal _laborCost;
    private decimal _externalServiceCost;
    private decimal _overheadCost;
    private decimal _totalCost;
    private decimal _targetProfitAmount;
    private decimal _recommendedNetPrice;
    private string _statusText = "Bereit";

    public CalculationViewModel(
        CalculationService calculations,
        OfferService offers,
        AuthorizationService authorization,
        MaterialService materials,
        PurchaseListService purchaseListService)
    {
        _calculations = calculations;
        _offers = offers;
        _authorization = authorization;
        _materials = materials;
        _purchaseListService = purchaseListService;

        AddItemCommand = new AsyncCommand(AddItemAsync, CanEdit);
        UpdateItemCommand = new AsyncCommand(UpdateItemAsync, CanEditSelectedItem);
        RemoveItemCommand = new AsyncCommand(RemoveItemAsync, CanEditSelectedItem);
        ApplyProfitTargetCommand = new AsyncCommand(ApplyProfitTargetAsync, CanEdit);
        RefreshCommand = new AsyncCommand(LoadAsync);
        AddMaterialCommand = new AsyncCommand(AddMaterialAsync, CanAddMaterial);
        CreatePurchaseListCommand = new AsyncCommand(CreatePurchaseListAsync, CanEdit);

        _ = InitializeAsync();
    }

    public ObservableCollection<OfferDto> Offers { get; } = [];
    public ObservableCollection<CalculationItemDto> Items { get; } = [];
    public ObservableCollection<MaterialItemDto> Materials { get; } = [];
    public ObservableCollection<PurchaseListItemDto> PurchaseList { get; } = [];
    public IReadOnlyList<CostType> CostTypes { get; } = Enum.GetValues<CostType>();

    public ICommand AddItemCommand { get; }
    public ICommand UpdateItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ApplyProfitTargetCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddMaterialCommand { get; }
    public ICommand CreatePurchaseListCommand { get; }

    public OfferDto? SelectedOffer
    {
        get => _selectedOffer;
        set
        {
            if (Set(ref _selectedOffer, value))
            {
                _ = LoadAsync();
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

    public decimal MaterialQuantity { get => _materialQuantity; set => Set(ref _materialQuantity, value); }

    public CalculationItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value) && value is not null)
            {
                NewCostType = value.CostType;
                NewDescription = value.Description;
                NewQuantity = value.Quantity;
                NewUnitCost = value.UnitCost;
            }

            RefreshCommands();
        }
    }

    public CostType NewCostType { get => _newCostType; set => Set(ref _newCostType, value); }
    public string NewDescription { get => _newDescription; set => Set(ref _newDescription, value); }
    public decimal NewQuantity { get => _newQuantity; set => Set(ref _newQuantity, value); }
    public decimal NewUnitCost { get => _newUnitCost; set => Set(ref _newUnitCost, value); }
    public decimal ProfitTargetPercent { get => _profitTargetPercent; set => Set(ref _profitTargetPercent, value); }
    public decimal MaterialCost { get => _materialCost; private set => Set(ref _materialCost, value); }
    public decimal LaborCost { get => _laborCost; private set => Set(ref _laborCost, value); }
    public decimal ExternalServiceCost { get => _externalServiceCost; private set => Set(ref _externalServiceCost, value); }
    public decimal OverheadCost { get => _overheadCost; private set => Set(ref _overheadCost, value); }
    public decimal TotalCost { get => _totalCost; private set => Set(ref _totalCost, value); }
    public decimal TargetProfitAmount { get => _targetProfitAmount; private set => Set(ref _targetProfitAmount, value); }
    public decimal RecommendedNetPrice { get => _recommendedNetPrice; private set => Set(ref _recommendedNetPrice, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        var offers = await _offers.GetAllAsync();
        Offers.Clear();
        foreach (var offer in offers)
            Offers.Add(offer);

        var materials = await _materials.SearchAsync(null);
        Materials.Clear();
        foreach (var material in materials)
            Materials.Add(material);

        SelectedOffer = Offers.FirstOrDefault();
    }

    private async Task LoadAsync()
    {
        Items.Clear();

        if (SelectedOffer is null)
            return;

        try
        {
            var calculation = await _calculations.GetOrCreateAsync(SelectedOffer.Id);
            foreach (var item in calculation.Items)
                Items.Add(item);

            ProfitTargetPercent = calculation.ProfitTargetPercent;
            MaterialCost = calculation.MaterialCost;
            LaborCost = calculation.LaborCost;
            ExternalServiceCost = calculation.ExternalServiceCost;
            OverheadCost = calculation.OverheadCost;
            TotalCost = calculation.TotalCost;
            TargetProfitAmount = calculation.TargetProfitAmount;
            RecommendedNetPrice = calculation.RecommendedNetPrice;
            StatusText = $"Kalkulation für {SelectedOffer.OfferNumber} geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Kalkulation konnte nicht geladen werden");
        }
    }

    private async Task CreatePurchaseListAsync()
    {
        PurchaseList.Clear();

        if (SelectedOffer is null)
            return;

        try
        {
            var items = await _purchaseListService.CreateAsync(SelectedOffer.Id);
            foreach (var item in items)
                PurchaseList.Add(item);

            StatusText = $"{PurchaseList.Count} Bestellposition(en) erzeugt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellliste konnte nicht erzeugt werden");
        }
    }

    private async Task AddMaterialAsync()
    {
        if (SelectedOffer is null || SelectedMaterial is null)
            return;

        try
        {
            await _calculations.AddMaterialAsync(
                SelectedOffer.Id,
                SelectedMaterial.Id,
                MaterialQuantity);

            MaterialQuantity = 1m;
            await LoadAsync();
            StatusText = "Material wurde aus dem Materialstamm übernommen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Material konnte nicht übernommen werden");
        }
    }

    private async Task AddItemAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            await _calculations.AddItemAsync(
                SelectedOffer.Id,
                NewCostType,
                NewDescription,
                NewQuantity,
                NewUnitCost,
                materialItemId: null);

            ClearEditor();
            await LoadAsync();
            StatusText = "Kalkulationsposition wurde hinzugefügt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht hinzugefügt werden");
        }
    }

    private async Task UpdateItemAsync()
    {
        if (SelectedOffer is null || SelectedItem is null)
            return;

        try
        {
            await _calculations.UpdateItemAsync(
                SelectedOffer.Id,
                SelectedItem.Id,
                NewCostType,
                NewDescription,
                NewQuantity,
                NewUnitCost);

            await LoadAsync();
            StatusText = "Kalkulationsposition wurde aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht aktualisiert werden");
        }
    }

    private async Task RemoveItemAsync()
    {
        if (SelectedOffer is null || SelectedItem is null)
            return;

        try
        {
            await _calculations.RemoveItemAsync(
                SelectedOffer.Id,
                SelectedItem.Id);

            ClearEditor();
            await LoadAsync();
            StatusText = "Kalkulationsposition wurde entfernt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Position konnte nicht entfernt werden");
        }
    }

    private async Task ApplyProfitTargetAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            await _calculations.SetProfitTargetAsync(
                SelectedOffer.Id,
                ProfitTargetPercent);

            await LoadAsync();
            StatusText = "Firmenziel wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Firmenziel konnte nicht gespeichert werden");
        }
    }

    private void ClearEditor()
    {
        SelectedItem = null;
        NewCostType = CostType.Material;
        NewDescription = string.Empty;
        NewQuantity = 1m;
        NewUnitCost = 0m;
    }

    private bool CanEdit() =>
        _authorization.CanEditOffers() && SelectedOffer is not null;

    private bool CanAddMaterial() =>
        CanEdit() && SelectedMaterial is not null && MaterialQuantity > 0;

    private bool CanEditSelectedItem() =>
        CanEdit() && SelectedItem is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            AddItemCommand,
            UpdateItemCommand,
            RemoveItemCommand,
            ApplyProfitTargetCommand,
            AddMaterialCommand,
            CreatePurchaseListCommand
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
