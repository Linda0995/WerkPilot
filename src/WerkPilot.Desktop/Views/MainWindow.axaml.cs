using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace WerkPilot.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

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
