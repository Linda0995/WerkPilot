# Eingangsrechnungsprüfung

## Drei-Wege-Abgleich

WerkPilot vergleicht:

1. bestellte Menge und Bestellpreis
2. tatsächlich eingegangene Menge
3. verrechnete Menge und Rechnungspreis

## Prüfstatus

- Exakt: Mengen und Preise stimmen innerhalb der Toleranz
- Warnung: Preisabweichung über 2 Prozent oder sonstige nicht kritische Abweichung
- Kritisch: verrechnete Menge ist größer als der gebuchte Wareneingang

## Exportordner

```text
Dokumente/WerkPilot/Exporte/Eingangsrechnungen
```

Das CSV-Protokoll dokumentiert Bestellung, Wareneingang, Rechnungswerte und alle
ermittelten Abweichungen.
