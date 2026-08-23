using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Messaging;
using WerkPilot.Domain.Messaging;

namespace WerkPilot.Desktop.ViewModels;

public sealed class DocumentEmailViewModel : INotifyPropertyChanged
{
    private readonly DocumentEmailService _emailService;
    private readonly DocumentEmailCatalogService _catalogService;
    private readonly DocumentEmailTemplateService _templateService;
    private DocumentEmailType _selectedDocumentType =
        DocumentEmailType.CustomerInvoice;
    private DocumentEmailDocumentOption? _selectedDocument;
    private DocumentEmailTemplateDto? _selectedTemplate;
    private DocumentEmailDispatchDto? _selectedDispatch;
    private string _recipient = string.Empty;
    private string _subject = string.Empty;
    private string _body = string.Empty;
    private string _attachmentFileName = string.Empty;
    private string _templateName = string.Empty;
    private bool _templateIsDefault = true;
    private DateTimeOffset? _retryAt = DateTimeOffset.Now.AddMinutes(15);
    private string _smtpStatusText = "SMTP noch nicht geprüft";
    private string _queueStatusText = "Warteschlange noch nicht verarbeitet";
    private string _statusText = "Bereit";

    public DocumentEmailViewModel(
        DocumentEmailService emailService,
        DocumentEmailCatalogService catalogService,
        DocumentEmailTemplateService templateService)
    {
        _emailService = emailService;
        _catalogService = catalogService;
        _templateService = templateService;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        LoadPreviewCommand = new AsyncCommand(LoadPreviewAsync, HasDocument);
        SendCommand = new AsyncCommand(SendAsync, CanSend);
        SaveTemplateCommand = new AsyncCommand(SaveTemplateAsync, CanSaveTemplate);
        ApplyTemplateCommand = new AsyncCommand(ApplyTemplateAsync, HasTemplate);
        RetryCommand = new AsyncCommand(RetryAsync, CanRetry);
        ScheduleRetryCommand = new AsyncCommand(ScheduleRetryAsync, CanScheduleRetry);
        TestSmtpCommand = new AsyncCommand(TestSmtpAsync);
        ProcessQueueCommand = new AsyncCommand(ProcessQueueAsync);

        _ = InitializeAsync();
    }

    public IReadOnlyList<DocumentEmailType> DocumentTypes { get; } =
        Enum.GetValues<DocumentEmailType>();

    public ObservableCollection<DocumentEmailDocumentOption> Documents { get; } = [];
    public ObservableCollection<DocumentEmailDispatchDto> Dispatches { get; } = [];
    public ObservableCollection<DocumentEmailTemplateDto> Templates { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand LoadPreviewCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand SaveTemplateCommand { get; }
    public ICommand ApplyTemplateCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand ScheduleRetryCommand { get; }
    public ICommand TestSmtpCommand { get; }
    public ICommand ProcessQueueCommand { get; }

    public DocumentEmailType SelectedDocumentType
    {
        get => _selectedDocumentType;
        set
        {
            if (Set(ref _selectedDocumentType, value))
                _ = RefreshTypeDataAsync();
        }
    }

    public DocumentEmailDocumentOption? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (Set(ref _selectedDocument, value))
            {
                ClearPreview();
                RefreshCommands();
            }
        }
    }

    public DocumentEmailTemplateDto? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (Set(ref _selectedTemplate, value))
            {
                TemplateName = value?.Name ?? string.Empty;
                TemplateIsDefault = value?.IsDefault ?? true;
                RefreshCommands();
            }
        }
    }

    public DocumentEmailDispatchDto? SelectedDispatch
    {
        get => _selectedDispatch;
        set
        {
            Set(ref _selectedDispatch, value);
            RefreshCommands();
        }
    }

    public string Recipient
    {
        get => _recipient;
        set { Set(ref _recipient, value); RefreshCommands(); }
    }

    public string Subject
    {
        get => _subject;
        set { Set(ref _subject, value); RefreshCommands(); }
    }

    public string Body
    {
        get => _body;
        set { Set(ref _body, value); RefreshCommands(); }
    }

    public string AttachmentFileName
    {
        get => _attachmentFileName;
        private set => Set(ref _attachmentFileName, value);
    }

    public string TemplateName
    {
        get => _templateName;
        set { Set(ref _templateName, value); RefreshCommands(); }
    }

    public bool TemplateIsDefault
    {
        get => _templateIsDefault;
        set => Set(ref _templateIsDefault, value);
    }

    public DateTimeOffset? RetryAt
    {
        get => _retryAt;
        set { Set(ref _retryAt, value); RefreshCommands(); }
    }

    public string SmtpStatusText
    {
        get => _smtpStatusText;
        private set => Set(ref _smtpStatusText, value);
    }

    public string QueueStatusText
    {
        get => _queueStatusText;
        private set => Set(ref _queueStatusText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task InitializeAsync()
    {
        await RefreshTypeDataAsync();
        await RefreshDispatchesAsync();
    }

    private async Task RefreshAsync()
    {
        await RefreshTypeDataAsync();
        await RefreshDispatchesAsync();
        StatusText = "Belege, Vorlagen und Versandprotokoll wurden aktualisiert.";
    }

    private async Task RefreshTypeDataAsync()
    {
        await RefreshDocumentsAsync();
        await RefreshTemplatesAsync();
    }

    private async Task RefreshDocumentsAsync()
    {
        var selectedId = SelectedDocument?.DocumentId;
        Documents.Clear();

        foreach (var item in await _catalogService.GetAsync(SelectedDocumentType))
            Documents.Add(item);

        SelectedDocument = selectedId.HasValue
            ? Documents.FirstOrDefault(x => x.DocumentId == selectedId.Value)
            : Documents.FirstOrDefault();
    }

    private async Task RefreshTemplatesAsync()
    {
        var selectedId = SelectedTemplate?.Id;
        Templates.Clear();

        foreach (var template in (await _templateService.GetAllAsync())
                     .Where(x => x.DocumentType == SelectedDocumentType))
        {
            Templates.Add(template);
        }

        SelectedTemplate = selectedId.HasValue
            ? Templates.FirstOrDefault(x => x.Id == selectedId.Value)
            : Templates.FirstOrDefault(x => x.IsDefault) ?? Templates.FirstOrDefault();
    }

    private async Task RefreshDispatchesAsync()
    {
        var selectedId = SelectedDispatch?.Id;
        Dispatches.Clear();

        foreach (var dispatch in await _emailService.GetDispatchesAsync())
            Dispatches.Add(dispatch);

        SelectedDispatch = selectedId.HasValue
            ? Dispatches.FirstOrDefault(x => x.Id == selectedId.Value)
            : Dispatches.FirstOrDefault();
    }

    private async Task LoadPreviewAsync()
    {
        if (SelectedDocument is null)
            return;

        try
        {
            var preview = await _emailService.CreatePreviewAsync(
                SelectedDocument.DocumentType,
                SelectedDocument.DocumentId);

            Recipient = preview.Recipient;
            Subject = preview.Subject;
            Body = preview.Body;
            AttachmentFileName = preview.AttachmentFileName;
            StatusText = "E-Mail-Vorschau wurde geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorschau konnte nicht geladen werden");
        }
    }

    private Task ApplyTemplateAsync()
    {
        if (SelectedTemplate is null)
            return Task.CompletedTask;

        Subject = SelectedTemplate.SubjectTemplate;
        Body = SelectedTemplate.BodyTemplate;
        StatusText = "Vorlage wurde in die Bearbeitung übernommen.";
        return Task.CompletedTask;
    }

    private async Task SaveTemplateAsync()
    {
        try
        {
            var saved = await _templateService.SaveAsync(
                new SaveDocumentEmailTemplateRequest(
                    SelectedTemplate?.Id,
                    SelectedDocumentType,
                    TemplateName,
                    Subject,
                    Body,
                    TemplateIsDefault));

            await RefreshTemplatesAsync();
            SelectedTemplate = Templates.FirstOrDefault(x => x.Id == saved.Id);
            StatusText = $"Vorlage „{saved.Name}“ wurde gespeichert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Vorlage konnte nicht gespeichert werden");
        }
    }

    private async Task SendAsync()
    {
        if (SelectedDocument is null)
            return;

        try
        {
            await _emailService.SendAsync(
                new SendDocumentEmailRequest(
                    SelectedDocument.DocumentType,
                    SelectedDocument.DocumentId,
                    Recipient,
                    Subject,
                    Body));

            await RefreshDispatchesAsync();
            StatusText = $"{SelectedDocument.DocumentNumber} wurde an {Recipient} versendet.";
        }
        catch (Exception ex)
        {
            await RefreshDispatchesAsync();
            StatusText = UiErrorFormatter.Format(ex, "Versand fehlgeschlagen");
        }
    }

    private async Task RetryAsync()
    {
        if (SelectedDispatch is null)
            return;

        try
        {
            await _emailService.RetryAsync(SelectedDispatch.Id);
            await RefreshDispatchesAsync();
            StatusText = "Versand wurde erfolgreich wiederholt.";
        }
        catch (Exception ex)
        {
            await RefreshDispatchesAsync();
            StatusText = UiErrorFormatter.Format(ex, "Erneuter Versand fehlgeschlagen");
        }
    }

    private async Task ScheduleRetryAsync()
    {
        if (SelectedDispatch is null || !RetryAt.HasValue)
            return;

        try
        {
            await _emailService.ScheduleRetryAsync(
                SelectedDispatch.Id,
                RetryAt.Value.ToUniversalTime());

            await RefreshDispatchesAsync();
            StatusText = $"Erneuter Versand wurde für {RetryAt.Value:g} vorgemerkt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Wiederholung konnte nicht vorgemerkt werden");
        }
    }


    private async Task TestSmtpAsync()
    {
        try
        {
            var result = await _emailService.TestSmtpAsync();

            SmtpStatusText =
                $"{result.Host}:{result.Port} · SSL: {result.EnableSsl} · "
                + (result.NetworkReachable ? "erreichbar" : "nicht erreichbar");

            StatusText = result.Message;
        }
        catch (Exception ex)
        {
            SmtpStatusText = "SMTP-Prüfung fehlgeschlagen";
            StatusText = UiErrorFormatter.Format(ex, "SMTP konnte nicht geprüft werden");
        }
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            var result = await _emailService.ProcessDueRetriesAsync();

            QueueStatusText =
                $"{result.DueCount} fällig · {result.SentCount} erfolgreich · "
                + $"{result.FailedCount} fehlgeschlagen";

            await RefreshDispatchesAsync();
            StatusText = "Fällige Versandwiederholungen wurden verarbeitet.";
        }
        catch (Exception ex)
        {
            QueueStatusText = "Warteschlangenverarbeitung fehlgeschlagen";
            StatusText = UiErrorFormatter.Format(ex, "Warteschlange konnte nicht verarbeitet werden");
        }
    }

    private bool HasDocument() => SelectedDocument is not null;
    private bool HasTemplate() => SelectedTemplate is not null;

    private bool CanSend() =>
        SelectedDocument is not null &&
        !string.IsNullOrWhiteSpace(Recipient) &&
        !string.IsNullOrWhiteSpace(Subject) &&
        !string.IsNullOrWhiteSpace(Body);

    private bool CanSaveTemplate() =>
        !string.IsNullOrWhiteSpace(TemplateName) &&
        !string.IsNullOrWhiteSpace(Subject) &&
        !string.IsNullOrWhiteSpace(Body);

    private bool CanRetry() =>
        SelectedDispatch?.Status == DocumentEmailStatus.Failed;

    private bool CanScheduleRetry() =>
        CanRetry() &&
        RetryAt.HasValue &&
        RetryAt.Value > DateTimeOffset.Now;

    private void ClearPreview()
    {
        Recipient = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        AttachmentFileName = string.Empty;
    }

    private void RefreshCommands()
    {
        foreach (var command in new[]
        {
            LoadPreviewCommand,
            SendCommand,
            SaveTemplateCommand,
            ApplyTemplateCommand,
            RetryCommand,
            ScheduleRetryCommand
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
