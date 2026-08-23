using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Identity;
using WerkPilot.Domain.Identity;

namespace WerkPilot.Desktop.ViewModels;

public sealed class UserAbsenceViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;
    private readonly UserAbsenceService _service;
    private UserDto? _selectedUser;
    private UserDto? _selectedSubstitute;
    private UserAbsenceDto? _selectedAbsence;
    private UserAbsenceType _absenceType = UserAbsenceType.Vacation;
    private DateTimeOffset? _startDate = DateTimeOffset.Now.Date;
    private DateTimeOffset? _endDate = DateTimeOffset.Now.Date.AddDays(1);
    private string? _note;
    private string _conflictText = "Keine Abwesenheit ausgewählt.";
    private string _transferReason = string.Empty;
    private string _statusText = "Bereit";

    public UserAbsenceViewModel(
        UserService userService,
        UserAbsenceService service)
    {
        _userService = userService;
        _service = service;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanCreate);
        CancelCommand = new AsyncCommand(CancelAsync, HasSelectedAbsence);
        CheckConflictCommand = new AsyncCommand(CheckConflictAsync, HasSelectedAbsence);
        TransferDueCommand = new AsyncCommand(
            () => TransferAsync(onlyDueDuringAbsence: true),
            CanTransfer);
        TransferAllCommand = new AsyncCommand(
            () => TransferAsync(onlyDueDuringAbsence: false),
            CanTransfer);

        _ = InitializeAsync();
    }

    public ObservableCollection<UserDto> Users { get; } = [];
    public ObservableCollection<UserAbsenceDto> Absences { get; } = [];
    public ObservableCollection<UserAbsenceAffectedWorkItemDto> AffectedWork { get; } = [];
    public IReadOnlyList<UserAbsenceType> AbsenceTypes { get; } =
        Enum.GetValues<UserAbsenceType>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CheckConflictCommand { get; }
    public ICommand TransferDueCommand { get; }
    public ICommand TransferAllCommand { get; }

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set { Set(ref _selectedUser, value); RefreshCommands(); }
    }

    public UserDto? SelectedSubstitute
    {
        get => _selectedSubstitute;
        set { Set(ref _selectedSubstitute, value); RefreshCommands(); }
    }

    public UserAbsenceDto? SelectedAbsence
    {
        get => _selectedAbsence;
        set
        {
            if (Set(ref _selectedAbsence, value))
            {
                RefreshCommands();
                _ = CheckConflictAsync();
            }
        }
    }

    public UserAbsenceType AbsenceType
    {
        get => _absenceType;
        set => Set(ref _absenceType, value);
    }

    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set { Set(ref _startDate, value); RefreshCommands(); }
    }

    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set { Set(ref _endDate, value); RefreshCommands(); }
    }

    public string? Note
    {
        get => _note;
        set => Set(ref _note, value);
    }


    public string TransferReason
    {
        get => _transferReason;
        set
        {
            Set(ref _transferReason, value);
            RefreshCommands();
        }
    }

    public string ConflictText
    {
        get => _conflictText;
        private set => Set(ref _conflictText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task InitializeAsync()
    {
        foreach (var user in (await _userService.GetAllAsync())
                     .Where(x => x.IsActive)
                     .OrderBy(x => x.DisplayName))
        {
            Users.Add(user);
        }

        SelectedUser = Users.FirstOrDefault();
        SelectedSubstitute = Users.Skip(1).FirstOrDefault();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var selectedId = SelectedAbsence?.Id;
            Absences.Clear();

            foreach (var item in await _service.GetAllAsync(
                         DateOnly.FromDateTime(DateTime.Today)))
            {
                Absences.Add(item);
            }

            SelectedAbsence = selectedId.HasValue
                ? Absences.FirstOrDefault(x => x.Id == selectedId.Value)
                : Absences.FirstOrDefault();

            StatusText = $"{Absences.Count} Abwesenheit(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Abwesenheiten konnten nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedUser is null || !StartDate.HasValue || !EndDate.HasValue)
            return;

        try
        {
            await _service.CreateAsync(
                new CreateUserAbsenceRequest(
                    SelectedUser.Id,
                    AbsenceType,
                    DateOnly.FromDateTime(StartDate.Value.DateTime),
                    DateOnly.FromDateTime(EndDate.Value.DateTime),
                    SelectedSubstitute?.Id,
                    Note));

            await RefreshAsync();
            StatusText = "Abwesenheit wurde angelegt.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Abwesenheit konnte nicht angelegt werden");
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedAbsence is null)
            return;

        try
        {
            await _service.CancelAsync(SelectedAbsence.Id);
            await RefreshAsync();
            StatusText = "Abwesenheit wurde storniert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Abwesenheit konnte nicht storniert werden");
        }
    }

    private async Task CheckConflictAsync()
    {
        if (SelectedAbsence is null)
        {
            AffectedWork.Clear();
            ConflictText = "Keine Abwesenheit ausgewählt.";
            return;
        }

        try
        {
            var conflict = await _service.GetConflictAsync(SelectedAbsence.Id);
            var preview = await _service.GetWorkPreviewAsync(SelectedAbsence.Id);

            AffectedWork.Clear();
            foreach (var item in preview.Items)
                AffectedWork.Add(item);

            ConflictText =
                $"{conflict.OpenWorkCount} offene Aufgabe(n): "
                + $"{conflict.OpenCustomerFollowUps} Kunden, "
                + $"{conflict.OpenProjectTasks} Projekte. "
                + $"{conflict.DueDuringAbsence} davon im Abwesenheitszeitraum fällig. "
                + (conflict.HasSubstitute
                    ? $"Vertretung: {preview.SubstituteDisplayName}."
                    : "Keine Vertretung eingetragen.");
        }
        catch (Exception ex)
        {
            ConflictText = $"Konfliktprüfung fehlgeschlagen: {ex.Message}";
        }
    }


    private async Task TransferAsync(bool onlyDueDuringAbsence)
    {
        if (SelectedAbsence is null)
            return;

        try
        {
            var result = await _service.TransferToSubstituteAsync(
                SelectedAbsence.Id,
                onlyDueDuringAbsence,
                TransferReason);

            await CheckConflictAsync();

            StatusText =
                $"{result.TotalTransferred} Aufgabe(n) an {result.SubstituteUserName} übergeben "
                + $"({result.CustomerFollowUpsTransferred} Kunden, "
                + $"{result.ProjectTasksTransferred} Projekte).";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Aufgaben konnten nicht übergeben werden");
        }
    }

    private bool CanTransfer() =>
        SelectedAbsence is not null
        && SelectedAbsence.SubstituteUserId.HasValue
        && SelectedAbsence.Status != UserAbsenceStatus.Cancelled
        && !string.IsNullOrWhiteSpace(TransferReason);

    private bool CanCreate() =>
        SelectedUser is not null
        && StartDate.HasValue
        && EndDate.HasValue
        && EndDate.Value.Date >= StartDate.Value.Date
        && SelectedSubstitute?.Id != SelectedUser.Id;

    private bool HasSelectedAbsence() => SelectedAbsence is not null;

    private void RefreshCommands()
    {
        (CreateCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (CheckConflictCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (TransferDueCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (TransferAllCommand as AsyncCommand)?.RaiseCanExecuteChanged();
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
            if (!CanExecute(parameter)) return;
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
