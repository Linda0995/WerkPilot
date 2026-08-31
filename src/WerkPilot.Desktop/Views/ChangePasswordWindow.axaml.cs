using Avalonia.Controls;
using Avalonia.Interactivity;
namespace WerkPilot.Desktop.Views;
public partial class ChangePasswordWindow : Window
{
    public ChangePasswordWindow() => InitializeComponent();
    private void SetPasswordVisibility(bool visible)
    {
        var c = visible ? '\0' : '●';
        CurrentPasswordBox.PasswordChar = c;
        NewPasswordBox.PasswordChar = c;
        ConfirmationBox.PasswordChar = c;
    }
    private void ShowPasswords_Checked(object? sender, RoutedEventArgs e) => SetPasswordVisibility(true);
    private void ShowPasswords_Unchecked(object? sender, RoutedEventArgs e) => SetPasswordVisibility(false);
}
