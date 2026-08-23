using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Documents;

namespace WerkPilot.Desktop.ViewModels;

public sealed class DocumentsViewModel : INotifyPropertyChanged
{
    private readonly DocumentService _documents;
    private readonly ProjectService _projects;
    private ProjectDto? _selectedProject;
    private DocumentFolderDto? _selectedFolder;
    private DocumentFileDto? _selectedFile;
    private string _newFolderName = string.Empty;
    private string _importPath = string.Empty;
    private string _importDisplayName = string.Empty;
    private string _renameFolderName = string.Empty;
    private string _renameFileName = string.Empty;
    private DocumentFolderDto? _targetFolder;
    private bool _includeDeleted;
    private string _statusText = "Bereit";

    public DocumentsViewModel(
        DocumentService documents,
        ProjectService projects)
    {
        _documents = documents;
        _projects = projects;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateFolderCommand = new AsyncCommand(CreateFolderAsync, HasProject);
        SelectFilesCommand = new RelayCommand(
            () => SelectFilesRequested?.Invoke(this, EventArgs.Empty));
        ImportFileCommand = new AsyncCommand(ImportFileAsync, CanImport);
        RenameFolderCommand = new AsyncCommand(RenameFolderAsync, HasSelectedFolder);
        RenameFileCommand = new AsyncCommand(RenameFileAsync, HasSelectedFile);
        MoveFileCommand = new AsyncCommand(MoveFileAsync, HasSelectedFile);
        MoveFolderCommand = new AsyncCommand(MoveFolderAsync, HasSelectedFolder);
        OpenFileCommand = new AsyncCommand(OpenFileAsync, HasSelectedFile);
        TrashFileCommand = new AsyncCommand(TrashFileAsync, HasSelectedFile);
        RestoreFileCommand = new AsyncCommand(RestoreFileAsync, HasSelectedFile);
        TrashFolderCommand = new AsyncCommand(TrashFolderAsync, HasSelectedFolder);
        RestoreFolderCommand = new AsyncCommand(RestoreFolderAsync, HasSelectedFolder);

        _ = InitializeAsync();
    }

    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<DocumentFolderDto> Folders { get; } = [];
    public ObservableCollection<DocumentFileDto> Files { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CreateFolderCommand { get; }
    public ICommand SelectFilesCommand { get; }
    public ICommand ImportFileCommand { get; }
    public ICommand RenameFolderCommand { get; }
    public ICommand RenameFileCommand { get; }
    public ICommand MoveFileCommand { get; }
    public ICommand MoveFolderCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand TrashFileCommand { get; }
    public ICommand RestoreFileCommand { get; }
    public ICommand TrashFolderCommand { get; }
    public ICommand RestoreFolderCommand { get; }

    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (Set(ref _selectedProject, value))
            {
                _ = RefreshAsync();
                RefreshCommands();
            }
        }
    }

    public DocumentFolderDto? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (Set(ref _selectedFolder, value))
                RenameFolderName = value?.Name ?? string.Empty;

            RefreshCommands();
        }
    }

    public DocumentFileDto? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (Set(ref _selectedFile, value))
                RenameFileName = value?.DisplayName ?? string.Empty;

            RefreshCommands();
        }
    }

    public string NewFolderName { get => _newFolderName; set => Set(ref _newFolderName, value); }
    public string RenameFolderName { get => _renameFolderName; set => Set(ref _renameFolderName, value); }
    public string RenameFileName { get => _renameFileName; set => Set(ref _renameFileName, value); }

    public DocumentFolderDto? TargetFolder
    {
        get => _targetFolder;
        set => Set(ref _targetFolder, value);
    }

    public string ImportPath { get => _importPath; set { Set(ref _importPath, value); RefreshCommands(); } }
    public string ImportDisplayName { get => _importDisplayName; set => Set(ref _importDisplayName, value); }

    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set
        {
            if (Set(ref _includeDeleted, value))
                _ = RefreshAsync();
        }
    }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    public event EventHandler? SelectFilesRequested;

    private async Task InitializeAsync()
    {
        var projects = await _projects.GetAllAsync();
        Projects.Clear();

        foreach (var project in projects)
            Projects.Add(project);

        SelectedProject = Projects.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        Folders.Clear();
        Files.Clear();

        if (SelectedProject is null)
            return;

        try
        {
            var folders = await _documents.GetFoldersAsync(
                DocumentOwnerType.Project,
                SelectedProject.Id,
                IncludeDeleted);

            var files = await _documents.GetFilesAsync(
                DocumentOwnerType.Project,
                SelectedProject.Id,
                IncludeDeleted);

            foreach (var folder in folders)
                Folders.Add(folder);

            foreach (var file in files)
                Files.Add(file);

            StatusText = $"{Folders.Count} Ordner und {Files.Count} Datei(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Projektakte konnte nicht geladen werden");
        }
    }

    private async Task CreateFolderAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            await _documents.CreateFolderAsync(
                NewFolderName,
                DocumentOwnerType.Project,
                SelectedProject.Id,
                SelectedFolder?.Id);

            NewFolderName = string.Empty;
            await RefreshAsync();
            StatusText = "Ordner wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ordner konnte nicht angelegt werden");
        }
    }

    public async Task ImportSelectedFilesAsync(
        IReadOnlyList<SelectedDocumentFile> selectedFiles)
    {
        if (SelectedProject is null || selectedFiles.Count == 0)
            return;

        try
        {
            var requests = selectedFiles
                .Select(file => new ImportDocumentFileRequest(
                    file.LocalPath,
                    file.SuggestedDisplayName,
                    DocumentOwnerType.Project,
                    SelectedProject.Id,
                    SelectedFolder?.Id))
                .ToArray();

            await _documents.ImportFilesAsync(requests);
            await RefreshAsync();

            StatusText = $"{requests.Length} Datei(en) wurden importiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Mehrfachimport fehlgeschlagen");
        }
    }

    private async Task ImportFileAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            var displayName = string.IsNullOrWhiteSpace(ImportDisplayName)
                ? Path.GetFileName(ImportPath)
                : ImportDisplayName;

            await _documents.ImportFileAsync(
                ImportPath,
                displayName,
                DocumentOwnerType.Project,
                SelectedProject.Id,
                SelectedFolder?.Id);

            ImportPath = string.Empty;
            ImportDisplayName = string.Empty;
            await RefreshAsync();
            StatusText = "Datei wurde in die Projektakte importiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Dateiimport fehlgeschlagen");
        }
    }

    private async Task RenameFolderAsync()
    {
        if (SelectedFolder is null)
            return;

        try
        {
            await _documents.RenameFolderAsync(SelectedFolder.Id, RenameFolderName);
            await RefreshAsync();
            StatusText = "Ordner wurde umbenannt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ordner konnte nicht umbenannt werden");
        }
    }

    private async Task RenameFileAsync()
    {
        if (SelectedFile is null)
            return;

        try
        {
            await _documents.RenameFileAsync(SelectedFile.Id, RenameFileName);
            await RefreshAsync();
            StatusText = "Datei wurde umbenannt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Datei konnte nicht umbenannt werden");
        }
    }

    private async Task MoveFileAsync()
    {
        if (SelectedFile is null)
            return;

        try
        {
            await _documents.MoveFileAsync(SelectedFile.Id, TargetFolder?.Id);
            await RefreshAsync();
            StatusText = TargetFolder is null
                ? "Datei wurde in den Hauptbereich verschoben."
                : $"Datei wurde nach „{TargetFolder.Name}“ verschoben.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Datei konnte nicht verschoben werden");
        }
    }

    private async Task MoveFolderAsync()
    {
        if (SelectedFolder is null)
            return;

        try
        {
            await _documents.MoveFolderAsync(SelectedFolder.Id, TargetFolder?.Id);
            await RefreshAsync();
            StatusText = TargetFolder is null
                ? "Ordner wurde in den Hauptbereich verschoben."
                : $"Ordner wurde nach „{TargetFolder.Name}“ verschoben.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ordner konnte nicht verschoben werden");
        }
    }

    private async Task OpenFileAsync()
    {
        if (SelectedFile is null)
            return;

        try
        {
            var path = await _documents.GetAbsolutePathAsync(SelectedFile.Id);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = "Datei wurde geöffnet.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Datei konnte nicht geöffnet werden");
        }
    }

    private async Task TrashFileAsync()
    {
        if (SelectedFile is null)
            return;

        await _documents.MoveFileToTrashAsync(SelectedFile.Id);
        await RefreshAsync();
        StatusText = "Datei wurde in den Papierkorb verschoben.";
    }

    private async Task RestoreFileAsync()
    {
        if (SelectedFile is null)
            return;

        await _documents.RestoreFileAsync(SelectedFile.Id);
        await RefreshAsync();
        StatusText = "Datei wurde wiederhergestellt.";
    }

    private async Task TrashFolderAsync()
    {
        if (SelectedFolder is null)
            return;

        await _documents.MoveFolderToTrashAsync(SelectedFolder.Id);
        await RefreshAsync();
        StatusText = "Ordner wurde in den Papierkorb verschoben.";
    }

    private async Task RestoreFolderAsync()
    {
        if (SelectedFolder is null)
            return;

        await _documents.RestoreFolderAsync(SelectedFolder.Id);
        await RefreshAsync();
        StatusText = "Ordner wurde wiederhergestellt.";
    }

    private bool HasProject() => SelectedProject is not null;
    private bool CanImport() => SelectedProject is not null && !string.IsNullOrWhiteSpace(ImportPath);
    private bool HasSelectedFile() => SelectedFile is not null;
    private bool HasSelectedFolder() => SelectedFolder is not null;

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            CreateFolderCommand,
            ImportFileCommand,
            RenameFolderCommand,
            RenameFileCommand,
            MoveFileCommand,
            MoveFolderCommand,
            OpenFileCommand,
            TrashFileCommand,
            RestoreFileCommand,
            TrashFolderCommand,
            RestoreFolderCommand
        })
            (command as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object? parameter) => execute();
    }

    private sealed class AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null) : ICommand
    {
        private bool _running;

        public bool CanExecute(object? parameter) =>
            !_running && (canExecute?.Invoke() ?? true);

        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _running = true;
                RaiseCanExecuteChanged();
                await execute();
            }
            finally
            {
                _running = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
