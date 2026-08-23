using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Inventory;

namespace WerkPilot.Desktop.ViewModels;

public sealed class InventoryCountViewModel : INotifyPropertyChanged
{
    private readonly InventoryCountService _service;
    private InventoryCountDto? _selectedCount;
    private InventoryCountLineDto? _selectedLine;
    private string _title = "Jahresinventur";
    private DateTimeOffset? _countDate = DateTimeOffset.Now;
    private string? _storageLocation;
    private decimal _countedQuantity;
    private string? _note;
    private string _statusText = "Bereit";

    public InventoryCountViewModel(InventoryCountService service)
    {
        _service = service;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        StartCommand = new AsyncCommand(StartAsync, HasCount);
        RecordCountCommand = new AsyncCommand(RecordCountAsync, HasLine);
        PostCommand = new AsyncCommand(PostAsync, HasCount);
        CancelCommand = new AsyncCommand(CancelAsync, HasCount);
        ExportCommand = new AsyncCommand(ExportAsync, HasCount);

        _ = RefreshAsync();
    }

    public ObservableCollection<InventoryCountDto> Counts { get; } = [];
    public ObservableCollection<InventoryCountLineDto> Lines { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand RecordCountCommand { get; }
    public ICommand PostCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ExportCommand { get; }

    public InventoryCountDto? SelectedCount
    {
        get => _selectedCount;
        set
        {
            if (Set(ref _selectedCount, value))
            {
                LoadLines(value);
                RefreshCommands();
            }
        }
    }

    public InventoryCountLineDto? SelectedLine
    {
        get => _selectedLine;
        set
        {
            if (Set(ref _selectedLine, value))
            {
                CountedQuantity = value?.CountedQuantity ?? value?.ExpectedQuantity ?? 0m;
                Note = value?.Note;
                RefreshCommands();
            }
        }
    }

    public string Title { get => _title; set { Set(ref _title, value); RefreshCommands(); } }
    public DateTimeOffset? CountDate { get => _countDate; set { Set(ref _countDate, value); RefreshCommands(); } }
    public string? StorageLocation { get => _storageLocation; set => Set(ref _storageLocation, value); }
    public decimal CountedQuantity { get => _countedQuantity; set => Set(ref _countedQuantity, value); }
    public string? Note { get => _note; set => Set(ref _note, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedCount?.Id;
            Counts.Clear();

            foreach (var count in await _service.GetAllAsync())
                Counts.Add(count);

            SelectedCount = selectedId.HasValue
                ? Counts.FirstOrDefault(x => x.Id == selectedId.Value)
                : Counts.FirstOrDefault();

            StatusText = $"{Counts.Count} Inventur(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventuren konnten nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (!CountDate.HasValue)
            return;

        try
        {
            var created = await _service.CreateAsync(
                Title,
                DateOnly.FromDateTime(CountDate.Value.DateTime),
                StorageLocation);

            await RefreshAsync();
            SelectedCount = Counts.FirstOrDefault(x => x.Id == created.Id);
            StatusText = $"Inventur {created.CountNumber} wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventur konnte nicht angelegt werden");
        }
    }

    private async Task StartAsync()
    {
        if (SelectedCount is null)
            return;

        try
        {
            await _service.StartAsync(SelectedCount.Id);
            await RefreshAsync();
            StatusText = "Inventur wurde zur Zählung freigegeben.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventur konnte nicht gestartet werden");
        }
    }

    private async Task RecordCountAsync()
    {
        if (SelectedCount is null || SelectedLine is null)
            return;

        try
        {
            await _service.RecordCountAsync(
                SelectedCount.Id,
                SelectedLine.Id,
                CountedQuantity,
                Note);

            await RefreshAsync();
            StatusText = "Zählmenge wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Zählmenge konnte nicht gespeichert werden");
        }
    }

    private async Task PostAsync()
    {
        if (SelectedCount is null)
            return;

        try
        {
            await _service.PostAsync(SelectedCount.Id);
            await RefreshAsync();
            StatusText = "Inventur wurde gebucht und der Lagerbestand korrigiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventur konnte nicht gebucht werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedCount is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedCount.Id);
            await RefreshAsync();
            StatusText = "Inventur wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventur konnte nicht storniert werden");
        }
    }

    private async Task ExportAsync()
    {
        if (SelectedCount is null)
            return;

        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WerkPilot",
                "Exporte",
                "Inventur");

            Directory.CreateDirectory(directory);

            var path = Path.Combine(
                directory,
                $"{SelectedCount.CountNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            await File.WriteAllTextAsync(
                path,
                await _service.ExportCsvAsync(SelectedCount.Id));

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });

            StatusText = $"Inventur wurde exportiert: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Inventur konnte nicht exportiert werden");
        }
    }

    private void LoadLines(InventoryCountDto? count)
    {
        Lines.Clear();

        if (count is not null)
            foreach (var line in count.Lines)
                Lines.Add(line);

        SelectedLine = Lines.FirstOrDefault();
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(Title) && CountDate.HasValue;

    private bool HasCount() => SelectedCount is not null;
    private bool HasLine() => SelectedCount is not null && SelectedLine is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateCommand,
            StartCommand,
            RecordCountCommand,
            PostCommand,
            CancelCommand,
            ExportCommand
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
