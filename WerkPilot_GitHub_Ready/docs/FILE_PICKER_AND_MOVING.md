# Dateiauswahl und Verschieben

## Nativer Dateiauswahldialog

WerkPilot verwendet Avalonia `StorageProvider.OpenFilePickerAsync`. Der Dialog unterstützt
Mehrfachauswahl und übernimmt ausschließlich lokale Dateien mit verfügbarem Dateipfad.

## Verschieben

Dateien und Ordner werden logisch über ihre Ordner-ID verschoben. Die physische Datei
bleibt an ihrem sicheren Speicherort. Dadurch entstehen keine Dateisystemfehler durch
Umbenennen oder Verschieben.

## Hauptbereich

Ein leeres Ziel entspricht dem Hauptbereich der Projektakte. Damit können Dateien und
Ordner aus Unterordnern wieder auf die oberste Ebene verschoben werden.
