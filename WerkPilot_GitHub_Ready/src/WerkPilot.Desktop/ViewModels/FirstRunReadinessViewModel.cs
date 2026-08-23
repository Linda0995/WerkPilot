using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WerkPilot.Desktop.ViewModels;

public sealed class FirstRunReadinessViewModel : INotifyPropertyChanged
{
    private readonly FirstRunReadinessService _service;
    private bool _databaseConfigured;
    private bool _databaseReachable;
    private bool _adminBootstrapConfigured;
    private bool _demoModeEnabled;
    private string _environmentName = string.Empty;
    private string _productVersion = string.Empty;
    private string _status = "Prüfung noch nicht ausgeführt.";
    private bool _isReady;

    public FirstRunReadinessViewModel(
        FirstRunReadinessService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ICommand RefreshCommand { get; }

    public bool DatabaseConfigured
    {
        get => _databaseConfigured;
        private set => Set(ref _databaseConfigured, value);
    }

    public bool DatabaseReachable
    {
        get => _databaseReachable;
        private set => Set(ref _databaseReachable, value);
    }

    public bool AdminBootstrapConfigured
    {
        get => _adminBootstrapConfigured;
        private set => Set(ref _adminBootstrapConfigured, value);
    }

    public bool DemoModeEnabled
    {
        get => _demoModeEnabled;
        private set => Set(ref _demoModeEnabled, value);
    }

    public string EnvironmentName
    {
        get => _environmentName;
        private set => Set(ref _environmentName, value);
    }

    public string ProductVersion
    {
        get => _productVersion;
        private set => Set(ref _productVersion, value);
    }

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public bool IsReady
    {
        get => _isReady;
        private set => Set(ref _isReady, value);
    }

    private async Task RefreshAsync()
    {
        try
        {
            var snapshot = await _service.CheckAsync();

            DatabaseConfigured = snapshot.DatabaseConfigured;
            DatabaseReachable = snapshot.DatabaseReachable;
            AdminBootstrapConfigured =
                snapshot.AdminBootstrapConfigured;
            DemoModeEnabled = snapshot.DemoModeEnabled;
            EnvironmentName = snapshot.EnvironmentName;
            ProductVersion = snapshot.ProductVersion;
            Status = snapshot.Status;
            IsReady = snapshot.IsReady;
        }
        catch (Exception ex)
        {
            Status = UiErrorFormatter.Format(
                ex,
                "Erststart-Prüfung konnte nicht ausgeführt werden");
            IsReady = false;
        }
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
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;

        public bool CanExecute(object? parameter) => !_running;
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (_running)
                return;

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
