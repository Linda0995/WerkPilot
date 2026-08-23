using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Identity;

namespace WerkPilot.Desktop.ViewModels;

public sealed class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthenticationService _authenticationService;
    private string _userName = "admin";
    private string _password = string.Empty;
    private string _statusText = "Bitte anmelden.";
    private bool _isBusy;

    public LoginViewModel(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new AsyncCommand(LoginAsync, () => !IsBusy);
    }

    public event EventHandler<AuthenticationResult>? LoginSucceeded;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand LoginCommand { get; }

    public string UserName { get => _userName; set => Set(ref _userName, value); }
    public string Password { get => _password; set => Set(ref _password, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) (LoginCommand as AsyncCommand)?.RaiseCanExecuteChanged(); } }

    private async Task LoginAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _authenticationService.LoginAsync(UserName, Password);
            StatusText = result.Message;
            if (result.Succeeded)
                LoginSucceeded?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Anmeldung fehlgeschlagen");
        }
        finally { IsBusy = false; }
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); return true;
    }

    private sealed class AsyncCommand(Func<Task> execute, Func<bool> canExecute) : ICommand
    {
        public bool CanExecute(object? parameter) => canExecute();
        public event EventHandler? CanExecuteChanged;
        public async void Execute(object? parameter) { if (CanExecute(parameter)) await execute(); }
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
