# Benutzerzuordnung von Projektaufgaben

Neue und bearbeitete Projektaufgaben speichern die eindeutige WerkPilot-Benutzer-ID.

Beim Datenbank-Upgrade versucht die Migration, ältere textuelle `AssignedTo`-Werte
über den Anzeigenamen vorhandener Benutzer aufzulösen.

Nicht eindeutig auflösbare Altbestände bleiben weiterhin über `AssignedTo` sichtbar
und werden durch den Legacy-Fallback berücksichtigt.
