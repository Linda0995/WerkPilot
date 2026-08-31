using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using WerkPilot.Desktop.ViewModels;
using Avalonia.Media;

namespace WerkPilot.Desktop.Views;

public partial class MainWindow : Window
{
    private GlobalSearchViewModel? _globalSearch;

    public MainWindow() => InitializeComponent();

    public void ConfigureGlobalSearch(GlobalSearchViewModel viewModel)
    {
        _globalSearch = viewModel;
        GlobalSearchHost.DataContext = viewModel;
        GlobalSearchResults.DataContext = viewModel;
        GlobalSearchGrid.DataContext = viewModel;
    }

    public void FocusGlobalSearch()
    {
        GlobalSearchBox.Focus();
        GlobalSearchResults.IsVisible = true;
    }

    private void GlobalSearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        GlobalSearchResults.IsVisible = true;
        if (e.Key == Key.Enter && _globalSearch?.SearchCommand.CanExecute(null) == true)
        {
            _globalSearch.SearchCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            GlobalSearchResults.IsVisible = false;
            e.Handled = true;
        }
    }

    private void GlobalSearchGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_globalSearch?.OpenResultCommand.CanExecute(null) == true)
            _globalSearch.OpenResultCommand.Execute(null);
    }

    public void ShowEmbedded(Window page, string title)
    {
        if (page.Content is not Control content)
            return;

        page.Content = null;
        content.DataContext = page.DataContext;

        var shell = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,*") };
        shell.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 26,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(6, 0, 0, 12)
        });
        var body = new Border { Child = content };
        Grid.SetRow(body, 1);
        shell.Children.Add(body);

        PageHost.Content = shell;
        DashboardPanel.IsVisible = false;
        PageHost.IsVisible = true;
    }

    private void Dashboard_Click(object? sender, RoutedEventArgs e)
    {
        PageHost.Content = null;
        PageHost.IsVisible = false;
        DashboardPanel.IsVisible = true;
    }
}
