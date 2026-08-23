# Architektur – WerkPilot 0.5

## Stil
Pragmatische Clean Architecture als modularer Monolith.

## Abhängigkeiten
- Domain kennt keine anderen Projekte.
- Application hängt nur von Domain ab.
- Infrastructure implementiert Application-Schnittstellen.
- Desktop ist der Composition Root und verbindet UI, Application und Infrastructure.

## Datenhaltung
PostgreSQL ist die verbindliche Datenbank. EF Core übernimmt Mapping und Persistenz.

## Löschkonzept
Fachliche Datensätze werden standardmäßig nur logisch gelöscht (`IsDeleted`).
Ein endgültiges Löschen wird erst in einem späteren Administrationsmodul umgesetzt.

## Erweiterung
Angebote, Kalkulation, Projekte, Dokumente, MES und KI werden als fachlich getrennte Module ergänzt,
ohne die bestehende Schichtenrichtung zu verletzen.
