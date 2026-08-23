namespace WerkPilot.Desktop.ViewModels;

public sealed record SelectedDocumentFile(
    string LocalPath,
    string SuggestedDisplayName);
