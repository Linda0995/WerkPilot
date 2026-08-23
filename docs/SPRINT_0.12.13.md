# Sprint 0.12.13 - RelayCommand CS0067 Fix

Der reale Windows-Build von 0.12.12 hat nur noch zwei Fehler gemeldet:

- `MainWindowViewModel.RelayCommand.CanExecuteChanged` – CS0067
- `DocumentsViewModel.RelayCommand.CanExecuteChanged` – CS0067

Beide RelayCommands besitzen immer `CanExecute == true` und benötigen daher
keine dynamische CanExecute-Benachrichtigung. Das ICommand-Event wird jetzt
mit leeren `add`/`remove`-Accessoren implementiert. Dadurch existiert kein
ungenutztes Event-Backing-Field mehr und CS0067 wird sauber vermieden.

Es wurde bewusst **keine globale Warnungsunterdrückung** eingebaut.
