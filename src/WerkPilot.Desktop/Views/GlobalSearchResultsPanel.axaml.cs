using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WerkPilot.Desktop.ViewModels;

namespace WerkPilot.Desktop.Views;

public partial class GlobalSearchResultsPanel : UserControl
{
    public GlobalSearchResultsPanel() => InitializeComponent();

    public event EventHandler? CloseRequested;

    private void Close_Click(object? sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void ResultsGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is GlobalSearchViewModel vm &&
            vm.OpenResultCommand.CanExecute(null))
            vm.OpenResultCommand.Execute(null);
    }
}
