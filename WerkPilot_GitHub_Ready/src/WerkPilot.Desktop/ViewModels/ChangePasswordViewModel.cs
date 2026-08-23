using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Identity;

namespace WerkPilot.Desktop.ViewModels;

public sealed class ChangePasswordViewModel : INotifyPropertyChanged
{
    private readonly AuthenticationService _authenticationService;
    private readonly Guid _userId;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmation = string.Empty;
    private string _statusText = "Bitte vergeben Sie ein neues Kennwort.";
    private bool _isBusy;

    public ChangePasswordViewModel(
        AuthenticationService authenticationService,
        Guid userId)
    {
        _authenticationService = authenticationService;
        _userId = userId;
        SaveCommand = new AsyncCommand(SaveAsync, () => !IsBusy);
    }

    public event EventHandler? PasswordChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand SaveCommand { get; }

    public string CurrentPassword { get => _currentPassword; set => Set(ref _currentPassword, value); }
    public string NewPassword { get => _newPassword; set => Set(ref _newPassword, value); }
    public string Confirmation { get => _confirmation; set => Set(ref _confirmation, value); }
    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
                (SaveCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            await _authenticationService.ChangePasswordAsync(new ChangePasswordRequest(
                _userId,
                CurrentPassword,
                NewPassword,
                Confirmation));

            StatusText = "Kennwort wurde erfolgreich geändert.";
            PasswordChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorgang fehlgeschlagen");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute, Func<bool> canExecute) : ICommand
    {
        public bool CanExecute(object? parameter) => canExecute();
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                await execute();
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
