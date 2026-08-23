# Sprint 0.12.23 - Verifier Cleanup

Der reale Lauf von 0.12.22 stoppte bereits in STEP 2. Ursache war keine Datenbank-
oder EF-Störung, sondern eine veraltete statische PrüfregeI aus 0.12.21.

Die alte Regel verlangte weiterhin eine direkte quoted Abfrage auf
`public."__EFMigrationsHistory"`, obwohl 0.12.22 diese absichtlich durch robuste
`information_schema`-/`pg_catalog`-Prüfungen ersetzt hatte.

0.12.23 entfernt diese widersprüchliche Altprüfung. Die neuen Katalogprüfungen
bleiben verbindlich bestehen.
