using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Search;
using WerkPilot.Application.Workbench;

namespace WerkPilot.Desktop.ViewModels;

public sealed class WorkbenchViewModel : INotifyPropertyChanged
{
    private readonly WorkbenchService _service;
    private WorkbenchItemDto? _selectedItem;
    private string _statusText = "Bereit";

    public WorkbenchViewModel(WorkbenchService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        ToggleFavoriteCommand = new AsyncCommand(ToggleFavoriteAsync, () => SelectedItem is not null);
        OpenCommand = new RelayCommand(OpenSelected, () => SelectedItem is not null);
        _ = RefreshAsync();
    }

    public ObservableCollection<WorkbenchItemDto> Favorites { get; } = [];
    public ObservableCollection<WorkbenchItemDto> Recent { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand OpenCommand { get; }
    public event EventHandler<GlobalSearchResult>? ItemOpenRequested;
    public event PropertyChangedEventHandler? PropertyChanged;

    public WorkbenchItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value))
            {
                (ToggleFavoriteCommand as AsyncCommand)?.RaiseCanExecuteChanged();
                (OpenCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task RefreshAsync()
    {
        try
        {
            Favorites.Clear();
            foreach (var item in await _service.GetFavoritesAsync())
                Favorites.Add(item);

            Recent.Clear();
            foreach (var item in await _service.GetRecentAsync())
                Recent.Add(item);

            StatusText = $"{Favorites.Count} Favorit(en), {Recent.Count} zuletzt verwendet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Schnellzugriff konnte nicht geladen werden");
        }
    }

    private async Task ToggleFavoriteAsync()
    {
        if (SelectedItem is null) return;
        await _service.ToggleFavoriteAsync(SelectedItem.Id);
        await RefreshAsync();
    }

    private void OpenSelected()
    {
        if (SelectedItem is null) return;
        if (!Enum.TryParse<SearchResultType>(SelectedItem.ItemType, out var type)) return;

        ItemOpenRequested?.Invoke(this, new GlobalSearchResult(
            type,
            SelectedItem.EntityId,
            SelectedItem.Title,
            SelectedItem.Subtitle ?? string.Empty,
            SelectedItem.Number,
            SelectedItem.ItemType,
            100));
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
        public event EventHandler? CanExecuteChanged;
        public void Execute(object? parameter) => execute();
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running && (canExecute?.Invoke() ?? true);
        public event EventHandler? CanExecuteChanged;
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            try { _running = true; RaiseCanExecuteChanged(); await execute(); }
            finally { _running = false; RaiseCanExecuteChanged(); }
        }
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
