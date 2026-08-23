using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.ProjectCosts;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.ProjectCosts;

namespace WerkPilot.Desktop.ViewModels;

public sealed class ProjectCostControllingViewModel : INotifyPropertyChanged
{
    private readonly ProjectService _projects;
    private readonly ProjectActualCostService _actualCosts;
    private readonly ProjectCostControllingService _controlling;
    private readonly ProjectProfitabilityService _profitability;
    private readonly ProjectClosingReportService _closingReport;
    private ProjectDto? _selectedProject;
    private ProjectActualCostDto? _selectedCost;
    private ProjectActualCostType _costType = ProjectActualCostType.Material;
    private string _description = string.Empty;
    private decimal _amountNet;
    private DateTimeOffset? _costDate = DateTimeOffset.Now;
    private string? _reference;
    private string _statusText = "Bereit";
    private string _controllingStatus = "Kein Budget";
    private string _profitabilityStatus = "Kein Verkaufspreis";

    public ProjectCostControllingViewModel(
        ProjectService projects,
        ProjectActualCostService actualCosts,
        ProjectCostControllingService controlling,
        ProjectProfitabilityService profitability,
        ProjectClosingReportService closingReport)
    {
        _projects = projects;
        _actualCosts = actualCosts;
        _controlling = controlling;
        _profitability = profitability;
        _closingReport = closingReport;

        RefreshCommand = new AsyncCommand(RefreshAsync);
        CreateCommand = new AsyncCommand(CreateAsync, CanSave);
        UpdateCommand = new AsyncCommand(UpdateAsync, HasSelectedCost);
        ExportClosingCsvCommand = new AsyncCommand(ExportClosingCsvAsync, HasProject);
        ExportClosingMarkdownCommand = new AsyncCommand(ExportClosingMarkdownAsync, HasProject);
        _ = InitializeAsync();
    }

    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public ObservableCollection<ProjectActualCostDto> ActualCosts { get; } = [];
    public IReadOnlyList<ProjectActualCostType> CostTypes { get; } =
        Enum.GetValues<ProjectActualCostType>();

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand ExportClosingCsvCommand { get; }
    public ICommand ExportClosingMarkdownCommand { get; }

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

    public ProjectActualCostDto? SelectedCost
    {
        get => _selectedCost;
        set
        {
            if (Set(ref _selectedCost, value) && value is not null)
            {
                CostType = value.CostType;
                Description = value.Description;
                AmountNet = value.AmountNet;
                CostDate = new DateTimeOffset(value.CostDate.ToDateTime(TimeOnly.MinValue));
                Reference = value.Reference;
            }
            RefreshCommands();
        }
    }

    public ProjectActualCostType CostType { get => _costType; set => Set(ref _costType, value); }
    public string Description { get => _description; set { Set(ref _description, value); RefreshCommands(); } }
    public decimal AmountNet { get => _amountNet; set { Set(ref _amountNet, value); RefreshCommands(); } }
    public DateTimeOffset? CostDate { get => _costDate; set => Set(ref _costDate, value); }
    public string? Reference { get => _reference; set => Set(ref _reference, value); }

    public decimal PlannedMaterialCost { get; private set; }
    public decimal ActualMaterialCost { get; private set; }
    public decimal PlannedLaborCost { get; private set; }
    public decimal ActualLaborCost { get; private set; }
    public decimal PlannedExternalServiceCost { get; private set; }
    public decimal ActualExternalServiceCost { get; private set; }
    public decimal PlannedOverheadCost { get; private set; }
    public decimal ActualOverheadCost { get; private set; }
    public decimal PlannedTotalCost { get; private set; }
    public decimal ActualTotalCost { get; private set; }
    public decimal VarianceAmount { get; private set; }
    public decimal RemainingBudget { get; private set; }
    public decimal UtilizationPercent { get; private set; }


    public decimal RevenueNet { get; private set; }
    public decimal PlannedContributionMargin { get; private set; }
    public decimal ActualContributionMargin { get; private set; }
    public decimal PlannedMarginPercent { get; private set; }
    public decimal ActualMarginPercent { get; private set; }
    public decimal ResultVariance { get; private set; }

    public string ProfitabilityStatus
    {
        get => _profitabilityStatus;
        private set => Set(ref _profitabilityStatus, value);
    }

    public string ControllingStatus
    {
        get => _controllingStatus;
        private set => Set(ref _controllingStatus, value);
    }

    public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

    private async Task InitializeAsync()
    {
        foreach (var project in await _projects.GetAllAsync())
            Projects.Add(project);

        SelectedProject = Projects.FirstOrDefault();
    }

    private async Task RefreshAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            ActualCosts.Clear();
            foreach (var cost in await _actualCosts.GetForProjectAsync(SelectedProject.Id))
                ActualCosts.Add(cost);

            var data = await _controlling.GetAsync(SelectedProject.Id);
            var profitability = await _profitability.GetAsync(SelectedProject.Id);

            SetSummary(data);
            SetProfitability(profitability);
            StatusText = $"{ActualCosts.Count} Ist-Kostenbuchung(en) geladen.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Kostencontrolling konnte nicht geladen werden");
        }
    }

    private async Task CreateAsync()
    {
        if (SelectedProject is null || !CostDate.HasValue)
            return;

        try
        {
            await _actualCosts.CreateAsync(
                SelectedProject.Id,
                CostType,
                Description,
                AmountNet,
                DateOnly.FromDateTime(CostDate.Value.DateTime),
                Reference);

            ClearEditor();
            await RefreshAsync();
            StatusText = "Ist-Kosten wurden erfasst.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ist-Kosten konnten nicht erfasst werden");
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedCost is null || !CostDate.HasValue)
            return;

        try
        {
            await _actualCosts.UpdateAsync(
                SelectedCost.Id,
                CostType,
                Description,
                AmountNet,
                DateOnly.FromDateTime(CostDate.Value.DateTime),
                Reference);

            await RefreshAsync();
            StatusText = "Ist-Kosten wurden aktualisiert.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Ist-Kosten konnten nicht aktualisiert werden");
        }
    }

    private void SetSummary(ProjectCostControllingDto data)
    {
        SetField(nameof(PlannedMaterialCost), PlannedMaterialCost = data.PlannedMaterialCost);
        SetField(nameof(ActualMaterialCost), ActualMaterialCost = data.ActualMaterialCost);
        SetField(nameof(PlannedLaborCost), PlannedLaborCost = data.PlannedLaborCost);
        SetField(nameof(ActualLaborCost), ActualLaborCost = data.ActualLaborCost);
        SetField(nameof(PlannedExternalServiceCost), PlannedExternalServiceCost = data.PlannedExternalServiceCost);
        SetField(nameof(ActualExternalServiceCost), ActualExternalServiceCost = data.ActualExternalServiceCost);
        SetField(nameof(PlannedOverheadCost), PlannedOverheadCost = data.PlannedOverheadCost);
        SetField(nameof(ActualOverheadCost), ActualOverheadCost = data.ActualOverheadCost);
        SetField(nameof(PlannedTotalCost), PlannedTotalCost = data.PlannedTotalCost);
        SetField(nameof(ActualTotalCost), ActualTotalCost = data.ActualTotalCost);
        SetField(nameof(VarianceAmount), VarianceAmount = data.VarianceAmount);
        SetField(nameof(RemainingBudget), RemainingBudget = data.RemainingBudget);
        SetField(nameof(UtilizationPercent), UtilizationPercent = data.UtilizationPercent);

        ControllingStatus = data.Status switch
        {
            ProjectCostControllingStatus.NoBudget => "Kein Kostenbudget vorhanden",
            ProjectCostControllingStatus.OnTrack => "Im Kostenbudget",
            ProjectCostControllingStatus.Warning => "Kostenbudget zu mindestens 85 % verbraucht",
            ProjectCostControllingStatus.Exceeded => "Kostenbudget überschritten",
            _ => data.Status.ToString()
        };
    }


    private void SetProfitability(ProjectProfitabilityDto data)
    {
        SetField(nameof(RevenueNet), RevenueNet = data.RevenueNet);
        SetField(nameof(PlannedContributionMargin), PlannedContributionMargin = data.PlannedContributionMargin);
        SetField(nameof(ActualContributionMargin), ActualContributionMargin = data.ActualContributionMargin);
        SetField(nameof(PlannedMarginPercent), PlannedMarginPercent = data.PlannedMarginPercent);
        SetField(nameof(ActualMarginPercent), ActualMarginPercent = data.ActualMarginPercent);
        SetField(nameof(ResultVariance), ResultVariance = data.ResultVariance);

        ProfitabilityStatus = data.Status switch
        {
            ProjectProfitabilityStatus.NoRevenue => "Kein Verkaufspreis vorhanden",
            ProjectProfitabilityStatus.Profitable => "Projekt profitabel",
            ProjectProfitabilityStatus.LowMargin => "Niedrige Marge unter 10 %",
            ProjectProfitabilityStatus.Loss => "Projekt aktuell im Verlust",
            _ => data.Status.ToString()
        };
    }

    private async Task ExportClosingCsvAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            var path = GetReportPath(SelectedProject.ProjectNumber, "csv");
            await File.WriteAllTextAsync(
                path,
                await _closingReport.ExportCsvAsync(SelectedProject.Id));

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"Projektabschlussbericht wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Abschlussbericht konnte nicht erstellt werden");
        }
    }

    private async Task ExportClosingMarkdownAsync()
    {
        if (SelectedProject is null)
            return;

        try
        {
            var path = GetReportPath(SelectedProject.ProjectNumber, "md");
            await File.WriteAllTextAsync(
                path,
                await _closingReport.ExportMarkdownAsync(SelectedProject.Id));

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusText = $"Projektabschlussbericht wurde erstellt: {path}";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Abschlussbericht konnte nicht erstellt werden");
        }
    }

    private static string GetReportPath(string projectNumber, string extension)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "WerkPilot",
            "Exporte",
            "Projektabschlüsse");

        Directory.CreateDirectory(directory);
        return Path.Combine(
            directory,
            $"{projectNumber}_Abschluss_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");
    }

    private void ClearEditor()
    {
        SelectedCost = null;
        CostType = ProjectActualCostType.Material;
        Description = string.Empty;
        AmountNet = 0m;
        CostDate = DateTimeOffset.Now;
        Reference = null;
    }

    private bool CanSave() =>
        SelectedProject is not null &&
        !string.IsNullOrWhiteSpace(Description) &&
        AmountNet >= 0 &&
        CostDate.HasValue;

    private bool HasSelectedCost() => SelectedCost is not null;
    private bool HasProject() => SelectedProject is not null;

    private void RefreshCommands()
    {
        (CreateCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (UpdateCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (ExportClosingCsvCommand as AsyncCommand)?.RaiseCanExecuteChanged();
        (ExportClosingMarkdownCommand as AsyncCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField(string propertyName, decimal value) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running && (canExecute?.Invoke() ?? true);
        public event EventHandler? CanExecuteChanged;
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            try { _running = true; RaiseCanExecuteChanged(); await execute(); }
            finally { _running = false; RaiseCanExecuteChanged(); }
        }
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
