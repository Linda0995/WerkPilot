using Avalonia.Controls;
using Avalonia.Input;
using WerkPilot.Desktop.ViewModels;

namespace WerkPilot.Desktop.Views;

public partial class GlobalSearchWindow : Window
{
    public GlobalSearchWindow() => InitializeComponent();

    private void OnResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is GlobalSearchViewModel viewModel &&
            viewModel.OpenResultCommand.CanExecute(null))
            viewModel.OpenResultCommand.Execute(null);
    }
}
