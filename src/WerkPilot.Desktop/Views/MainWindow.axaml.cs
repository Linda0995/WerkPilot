using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WerkPilot.Desktop.ViewModels;

namespace WerkPilot.Desktop.Views;

public partial class MainWindow : Window
{
    private GlobalSearchViewModel? _globalSearch;

    public MainWindow()
    {
        InitializeComponent();
        GlobalSearchBarControl.SearchActivated += (_, _) => GlobalSearchResultsControl.IsVisible = true;
        GlobalSearchBarControl.SearchDismissed += (_, _) => GlobalSearchResultsControl.IsVisible = false;
        GlobalSearchResultsControl.CloseRequested += (_, _) => GlobalSearchResultsControl.IsVisible = false;
    }

    public void ConfigureGlobalSearch(GlobalSearchViewModel viewModel)
    {
        _globalSearch = viewModel;
        GlobalSearchBarControl.DataContext = viewModel;
        GlobalSearchResultsControl.DataContext = viewModel;
    }

    public void FocusGlobalSearch()
    {
        GlobalSearchBarControl.FocusSearch();
    }

    public void CloseGlobalSearch()
    {
        GlobalSearchResultsControl.IsVisible = false;
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

    public void ShowDashboard()
    {
        PageHost.Content = null;
        PageHost.IsVisible = false;
        DashboardPanel.IsVisible = true;
    }

    private void Dashboard_Click(object? sender, RoutedEventArgs e)
    {
        ShowDashboard();
    }
}
