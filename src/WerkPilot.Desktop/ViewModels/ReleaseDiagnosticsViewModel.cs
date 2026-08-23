using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WerkPilot.Desktop.ViewModels;

public sealed class ReleaseDiagnosticsViewModel : INotifyPropertyChanged
{
    private readonly ReleaseDiagnosticsService _service;
    private string _productVersion = string.Empty;
    private string _informationalVersion = string.Empty;
    private string _dotnetRuntime = string.Empty;
    private string _operatingSystem = string.Empty;
    private string _processArchitecture = string.Empty;
    private string _environmentName = string.Empty;
    private string _baseDirectory = string.Empty;
    private string _logDirectory = string.Empty;
    private bool _databaseConnectionConfigured;
    private bool _initialAdminPasswordConfigured;
    private bool _demoDataEnabled;
    private string _statusText = "Bereit";

    public ReleaseDiagnosticsViewModel(
        ReleaseDiagnosticsService service)
    {
        _service = service;
        RefreshCommand = new RelayCommand(Refresh);
        Refresh();
    }

    public ICommand RefreshCommand { get; }

    public string ProductVersion
    {
        get => _productVersion;
        private set => Set(ref _productVersion, value);
    }

    public string InformationalVersion
    {
        get => _informationalVersion;
        private set => Set(ref _informationalVersion, value);
    }

    public string DotnetRuntime
    {
        get => _dotnetRuntime;
        private set => Set(ref _dotnetRuntime, value);
    }

    public string OperatingSystem
    {
        get => _operatingSystem;
        private set => Set(ref _operatingSystem, value);
    }

    public string ProcessArchitecture
    {
        get => _processArchitecture;
        private set => Set(ref _processArchitecture, value);
    }

    public string EnvironmentName
    {
        get => _environmentName;
        private set => Set(ref _environmentName, value);
    }

    public string BaseDirectory
    {
        get => _baseDirectory;
        private set => Set(ref _baseDirectory, value);
    }

    public string LogDirectory
    {
        get => _logDirectory;
        private set => Set(ref _logDirectory, value);
    }

    public bool DatabaseConnectionConfigured
    {
        get => _databaseConnectionConfigured;
        private set => Set(
            ref _databaseConnectionConfigured,
            value);
    }

    public bool InitialAdminBootstrapConfigured
    {
        get => _initialAdminPasswordConfigured;
        private set => Set(
            ref _initialAdminPasswordConfigured,
            value);
    }

    public bool DemoDataEnabled
    {
        get => _demoDataEnabled;
        private set => Set(ref _demoDataEnabled, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private void Refresh()
    {
        try
        {
            var snapshot = _service.Capture();

            ProductVersion = snapshot.ProductVersion;
            InformationalVersion =
                snapshot.InformationalVersion;
            DotnetRuntime = snapshot.DotnetRuntime;
            OperatingSystem = snapshot.OperatingSystem;
            ProcessArchitecture =
                snapshot.ProcessArchitecture;
            EnvironmentName = snapshot.EnvironmentName;
            BaseDirectory = snapshot.BaseDirectory;
            LogDirectory = snapshot.LogDirectory;
            DatabaseConnectionConfigured =
                snapshot.DatabaseConnectionConfigured;
            InitialAdminBootstrapConfigured =
                snapshot.InitialAdminBootstrapConfigured;
            DemoDataEnabled = snapshot.DemoDataEnabled;

            StatusText =
                $"Systemdiagnose aktualisiert: "
                + $"{snapshot.CheckedAtUtc:yyyy-MM-dd HH:mm:ss} UTC";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(
                ex,
                "Systemdiagnose konnte nicht aktualisiert werden");
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

    private sealed class RelayCommand(Action execute)
        : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object? parameter) => execute();
    }
}
