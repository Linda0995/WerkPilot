using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Purchasing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class PurchaseListsViewModel : INotifyPropertyChanged
{
    private readonly PurchaseListService _service;
    private readonly OfferService _offers;
    private PurchaseListDto? _selectedList;
    private PurchaseListItemDto? _selectedItem;
    private OfferDto? _selectedOffer;
    private string? _orderNote;
    private string _statusText = "Bereit";

    public PurchaseListsViewModel(
        PurchaseListService service,
        OfferService offers)
    {
        _service = service;
        _offers = offers;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, () => SelectedOffer is not null);
        ToggleOrderedCommand = new AsyncCommand(ToggleOrderedAsync, HasSelectedItem);
        SaveNoteCommand = new AsyncCommand(SaveNoteAsync, HasSelectedItem);
        ExportCsvCommand = new AsyncCommand(ExportCsvAsync, HasSelectedList);

        _ = InitializeAsync();
    }

    public ObservableCollection<PurchaseListDto> Lists { get; } = [];
    public ObservableCollection<PurchaseListItemDto> Items { get; } = [];
    public ObservableCollection<OfferDto> Offers { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand ToggleOrderedCommand { get; }
    public ICommand SaveNoteCommand { get; }
    public ICommand ExportCsvCommand { get; }

    public PurchaseListDto? SelectedList
    {
        get => _selectedList;
        set
        {
            if (Set(ref _selectedList, value))
            {
                LoadItems(value);
                RefreshCommands();
            }
        }
    }

    public PurchaseListItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value))
            {
                OrderNote = value?.OrderNote;
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

    public string? OrderNote { get => _orderNote; set => Set(ref _orderNote, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        var offers = await _offers.GetAllAsync();
        Offers.Clear();
        foreach (var offer in offers)
            Offers.Add(offer);

        SelectedOffer = Offers.FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedList?.Id;
            var lists = await _service.GetAllAsync();

            Lists.Clear();
            foreach (var list in lists)
                Lists.Add(list);

            SelectedList = selectedId is null
                ? Lists.FirstOrDefault()
                : Lists.FirstOrDefault(x => x.Id == selectedId);

            StatusText = $"{Lists.Count} Bestellliste(n) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestelllisten konnten nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedOffer is null)
            return;

        try
        {
            var created = await _service.CreateOrRefreshFromOfferAsync(SelectedOffer.Id);
            await RefreshAsync();
            SelectedList = Lists.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Bestellliste {created.PurchaseListNumber} ist bereit.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellliste konnte nicht erzeugt werden");
        }
    }

    private async Task ToggleOrderedAsync()
    {
        if (SelectedList is null || SelectedItem is null)
            return;

        try
        {
            await _service.ToggleOrderedAsync(
                SelectedList.Id,
                SelectedItem.Id,
                OrderNote);

            await RefreshAsync();
            StatusText = "Bestellstatus wurde geändert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellstatus konnte nicht geändert werden");
        }
    }

    private async Task SaveNoteAsync()
    {
        if (SelectedList is null || SelectedItem is null)
            return;

        try
        {
            await _service.UpdateNoteAsync(
                SelectedList.Id,
                SelectedItem.Id,
                OrderNote);

            await RefreshAsync();
            StatusText = "Bestellnotiz wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Bestellnotiz konnte nicht gespeichert werden");
        }
    }

    private async Task ExportCsvAsync()
    {
        if (SelectedList is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Bestelllisten");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedList.PurchaseListNumber}.csv");

            var csv = await _service.ExportCsvAsync(SelectedList.Id);
            await File.WriteAllTextAsync(path, csv);

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"CSV-Bestellliste wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "CSV-Export fehlgeschlagen");
        }
    }

    private void LoadItems(PurchaseListDto? list)
    {
        Items.Clear();

        if (list is not null)
            foreach (var item in list.Items)
                Items.Add(item);

        SelectedItem = null;
    }

    private bool HasSelectedItem() =>
        SelectedList is not null && SelectedItem is not null;

    private bool HasSelectedList() => SelectedList is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateCommand,
            ToggleOrderedCommand,
            SaveNoteCommand,
            ExportCsvCommand
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
