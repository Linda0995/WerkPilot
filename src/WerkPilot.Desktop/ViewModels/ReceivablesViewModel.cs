using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Billing;

namespace WerkPilot.Desktop.ViewModels;

public sealed class ReceivablesViewModel : INotifyPropertyChanged
{
    private readonly CustomerInvoiceService _service;
    private string _statusText = "Bereit";

    public ReceivablesViewModel(CustomerInvoiceService service)
    {
        _service = service;
        RefreshCommand = new AsyncCommand(RefreshAsync);
        _ = RefreshAsync();
    }

    public ObservableCollection<CustomerInvoiceDto> Items { get; } = [];
    public ICommand RefreshCommand { get; }

    public decimal TotalOpenAmount { get; private set; }
    public decimal OverdueAmount { get; private set; }
    public decimal DueWithin7Days { get; private set; }
    public decimal DueWithin14Days { get; private set; }
    public decimal DueWithin30Days { get; private set; }
    public int OpenInvoiceCount { get; private set; }
    public int OverdueInvoiceCount { get; private set; }

    public string StatusText
    {
        get => _statusText;
        private set => Set(ref _statusText, value);
    }

    private async Task RefreshAsync()
    {
        try
        {
            var summary = await _service.GetReceivablesSummaryAsync(
                DateOnly.FromDateTime(DateTime.Today));

            Items.Clear();
            foreach (var item in summary.Items)
                Items.Add(item);

            TotalOpenAmount = summary.TotalOpenAmount;
            OverdueAmount = summary.OverdueAmount;
            DueWithin7Days = summary.DueWithin7Days;
            DueWithin14Days = summary.DueWithin14Days;
            DueWithin30Days = summary.DueWithin30Days;
            OpenInvoiceCount = summary.OpenInvoiceCount;
            OverdueInvoiceCount = summary.OverdueInvoiceCount;

            foreach (var name in new[]
            {
                nameof(TotalOpenAmount), nameof(OverdueAmount),
                nameof(DueWithin7Days), nameof(DueWithin14Days),
                nameof(DueWithin30Days), nameof(OpenInvoiceCount),
                nameof(OverdueInvoiceCount)
            })
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            StatusText = $"{OpenInvoiceCount} offene Forderung(en), {OverdueInvoiceCount} überfällig.";
        }
        catch (Exception ex)
        {
            StatusText = UiErrorFormatter.Format(ex, "Forderungsübersicht konnte nicht geladen werden");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private sealed class AsyncCommand(Func<Task> execute) : ICommand
    {
        private bool _running;
        public bool CanExecute(object? parameter) => !_running;
        public event EventHandler? CanExecuteChanged;

        public async void Execute(object? parameter)
        {
            if (_running) return;

            try
            {
                _running = true;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                await execute();
            }
            finally
            {
                _running = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
