using Avalonia.Controls;
using Avalonia.Input;
using WerkPilot.Desktop.ViewModels;

namespace WerkPilot.Desktop.Views;

public partial class GlobalSearchBar : UserControl
{
    public GlobalSearchBar() => InitializeComponent();

    public event EventHandler? SearchActivated;
    public event EventHandler? SearchDismissed;

    public void FocusSearch()
    {
        SearchBox.Focus();
        SearchActivated?.Invoke(this, EventArgs.Empty);
    }

    private void SearchBox_GotFocus(object? sender, GotFocusEventArgs e)
        => SearchActivated?.Invoke(this, EventArgs.Empty);

    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            DataContext is GlobalSearchViewModel vm &&
            vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            SearchActivated?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SearchDismissed?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }
}
