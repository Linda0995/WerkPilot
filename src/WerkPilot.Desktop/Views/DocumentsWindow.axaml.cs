using Avalonia.Controls;
using Avalonia.Platform.Storage;
using WerkPilot.Desktop.ViewModels;

namespace WerkPilot.Desktop.Views;

public partial class DocumentsWindow : Window
{
    private DocumentsViewModel? _subscribedViewModel;

    public DocumentsWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs eventArgs)
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.SelectFilesRequested -= OnSelectFilesRequested;

        _subscribedViewModel = DataContext as DocumentsViewModel;

        if (_subscribedViewModel is not null)
            _subscribedViewModel.SelectFilesRequested += OnSelectFilesRequested;
    }

    private async void OnSelectFilesRequested(object? sender, EventArgs eventArgs)
    {
        if (_subscribedViewModel is null || !StorageProvider.CanOpen)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Dateien für die WerkPilot-Projektakte auswählen",
                AllowMultiple = true,
                FileTypeFilter =
                [
                    FilePickerFileTypes.All,
                    FilePickerFileTypes.Pdf,
                    FilePickerFileTypes.ImageAll,
                    new FilePickerFileType("CAD-Dateien")
                    {
                        Patterns = ["*.dxf", "*.dwg"]
                    },
                    new FilePickerFileType("Office-Dateien")
                    {
                        Patterns = ["*.docx", "*.xlsx", "*.csv", "*.txt"]
                    }
                ]
            });

        var selected = files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => new SelectedDocumentFile(
                path!,
                Path.GetFileName(path!)))
            .ToArray();

        await _subscribedViewModel.ImportSelectedFilesAsync(selected);
    }
}
