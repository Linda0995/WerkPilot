using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WerkPilot.Application.Notifications;

namespace WerkPilot.Desktop.ViewModels;

public sealed class NotificationsViewModel : INotifyPropertyChanged
{
 private readonly NotificationService _service; private NotificationItem? _selected; private string _status="Bereit";
 public NotificationsViewModel(NotificationService service){_service=service; RefreshCommand=new AsyncCommand(RefreshAsync); MarkReadCommand=new AsyncCommand(MarkReadAsync,()=>Selected is not null&&!Selected.IsRead); MarkAllReadCommand=new AsyncCommand(MarkAllReadAsync,()=>Items.Any(x=>!x.IsRead)); _=RefreshAsync();}
 public ObservableCollection<NotificationItem> Items {get;}=[];
 public ICommand RefreshCommand{get;} public ICommand MarkReadCommand{get;} public ICommand MarkAllReadCommand{get;}
 public NotificationItem? Selected{get=>_selected;set{Set(ref _selected,value);RefreshCommands();}}
 public int UnreadCount=>Items.Count(x=>!x.IsRead); public string StatusText{get=>_status;private set=>Set(ref _status,value);}
 private async Task RefreshAsync(){try{var items=await _service.GetAsync(DateOnly.FromDateTime(DateTime.Today));Items.Clear();foreach(var x in items)Items.Add(x);OnPropertyChanged(nameof(UnreadCount));RefreshCommands();StatusText=$"{Items.Count} Hinweis(e), {UnreadCount} ungelesen.";}catch(Exception ex){StatusText=UiErrorFormatter.Format(ex, "Benachrichtigungen konnten nicht geladen werden");}}
 private async Task MarkReadAsync(){if(Selected is null)return;await _service.MarkReadAsync(Selected.Key);await RefreshAsync();}
 private async Task MarkAllReadAsync(){await _service.MarkAllReadAsync(Items.Where(x=>!x.IsRead).Select(x=>x.Key));await RefreshAsync();}
 private void RefreshCommands(){(MarkReadCommand as AsyncCommand)?.Raise();(MarkAllReadCommand as AsyncCommand)?.Raise();}
 public event PropertyChangedEventHandler? PropertyChanged; private void OnPropertyChanged(string n)=>PropertyChanged?.Invoke(this,new(n));
 private bool Set<T>(ref T f,T v,[CallerMemberName]string? n=null){if(EqualityComparer<T>.Default.Equals(f,v))return false;f=v;OnPropertyChanged(n!);return true;}
 private sealed class AsyncCommand(Func<Task> e,Func<bool>? c=null):ICommand{bool r;public bool CanExecute(object? p)=>!r&&(c?.Invoke()??true);public event EventHandler? CanExecuteChanged;public async void Execute(object? p){if(!CanExecute(p))return;try{r=true;Raise();await e();}finally{r=false;Raise();}}public void Raise()=>CanExecuteChanged?.Invoke(this,EventArgs.Empty);}
}
