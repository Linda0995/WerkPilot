using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Search;

namespace WerkPilot.Desktop.ViewModels;

public sealed class GlobalSearchViewModel : INotifyPropertyChanged
{
    private readonly GlobalSearchService _service;
    private CancellationTokenSource? _searchCancellation;
    private string _searchText = string.Empty;
    private GlobalSearchResult? _selectedResult;
    private string _statusText = "Mindestens zwei Zeichen eingeben.";
    private bool _isSearching;

    public GlobalSearchViewModel(GlobalSearchService service)
    {
        _service = service;
        SearchCommand = new AsyncCommand(SearchNowAsync);
        OpenResultCommand = new RelayCommand(OpenSelected, () => SelectedResult is not null);
    }

    public ObservableCollection<GlobalSearchResult> Results { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand OpenResultCommand { get; }

    public event EventHandler<GlobalSearchResult>? ResultOpenRequested;
    public event PropertyChangedEventHandler? PropertyChanged;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value))
                _ = DebouncedSearchAsync();
        }
    }

    public GlobalSearchResult? SelectedResult
    {
        get => _selectedResult;
        set
        {
            Set(ref _selectedResult, value);
            (OpenResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }
    public bool IsSearching { get => _isSearching; private set => Set(ref _isSearching, value); }

    private async Task DebouncedSearchAsync()
    {
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = new CancellationTokenSource();
        var token = _searchCancellation.Token;

        try
        {
            await Task.Delay(300, token);
            await SearchAsync(token);
        }
        catch (OperationCanceledException) { }
    }

    private Task SearchNowAsync()
    {
        _searchCancellation?.Cancel();
        return SearchAsync(CancellationToken.None);
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        Results.Clear();
        if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Trim().Length < 2)
        {
            StatusText = "Mindestens zwei Zeichen eingeben.";
            return;
        }

        IsSearching = true;
        try
        {
            var results = await _service.SearchAsync(SearchText, 60, cancellationToken);
            foreach (var result in results)
                Results.Add(result);

            SelectedResult = Results.FirstOrDefault();
            StatusText = $"{Results.Count} Treffer gefunden.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            StatusText = UiErrorFormatter.Format(ex, "Suche fehlgeschlagen");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void OpenSelected()
    {
        if (SelectedResult is not null)
            ResultOpenRequested?.Invoke(this, SelectedResult);
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

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running;
        public event EventHandler? CanExecuteChanged;
        public async void Execute(object? parameter)
        {
            if (_running) return;
            try { _running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty); await execute(); }
            finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
        }
    }
}
