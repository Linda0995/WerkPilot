using Avalonia.Controls;
using Avalonia.Interactivity;
namespace WerkPilot.Desktop.Views;
public partial class LoginWindow : Window
{
    public LoginWindow() => InitializeComponent();
    private void ShowPassword_Checked(object? sender, RoutedEventArgs e) => PasswordBox.PasswordChar = '\0';
    private void ShowPassword_Unchecked(object? sender, RoutedEventArgs e) => PasswordBox.PasswordChar = '●';
}
