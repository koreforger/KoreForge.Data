# Development Guide

## Prerequisites

- .NET 10 SDK (10.0.200+)
- SQL Server instance (for scaffolding against a live database)

## Initial Setup

```shell
# Restore local tools (dotnet-ef, reportgenerator)
dotnet tool restore

# Build and run tests
.\bin\build-test.ps1
```

## Project Layout

```
src/KF.Data/
  Alerts/
    Generated/         ← Scaffold output (disposable)
    AlertsDbOptions.cs ← Manual: connection string holder
    AlertsDbServiceCollectionExtensions.cs ← Manual: DI registration

tst/KF.Data.Tests/     ← Unit tests (SQLite in-memory)

config/
  scaffold-config.json ← Drives scaffolding for all databases

scripts/
  scaffold-db.ps1      ← Reads config and runs dotnet ef scaffold
  AlertsDB-Notification-Schema.sql ← DDL for AlertsDB
```

## Scaffolding Workflow

### When the Database Schema Changes

1. Apply DDL changes to the SQL Server database
2. Run `.\scripts\scaffold-db.ps1`
3. Review changes in `Generated/` folders
4. Update tests if new entities/columns were added
5. Commit

### Adding a New Database

1. Create a SQL script under `scripts/`
2. Add an entry to `config/scaffold-config.json`:

```json
{
  "name": "NewDB",
  "connectionString": "Server=...;Database=NewDB;...",
  "provider": "Microsoft.EntityFrameworkCore.SqlServer",
  "context": "NewDbContext",
  "outputDir": "src/KF.Data/NewDb/Generated",
  "schemas": ["dbo"],
  "tables": ["dbo.Table1", "dbo.Table2"],
  "useDatabaseNames": true,
  "noOnConfiguring": true
}
```

3. Run `.\scripts\scaffold-db.ps1 -Database NewDB`
4. Create `src/KF.Data/NewDb/NewDbOptions.cs` and `NewDbServiceCollectionExtensions.cs`
5. Add tests and commit

### scaffold-config.json Format

| Field | Description |
|-------|-------------|
| `name` | Friendly name for the database |
| `connectionString` | SQL Server connection string |
| `provider` | EF Core provider (e.g. `Microsoft.EntityFrameworkCore.SqlServer`) |
| `context` | Name for the generated DbContext class |
| `outputDir` | Relative path for scaffold output |
| `schemas` | Array of schemas to include |
| `tables` | Array of fully-qualified table names |
| `useDatabaseNames` | Preserve database column/table names |
| `noOnConfiguring` | Do not generate OnConfiguring (connection is injected via DI) |

## Build Scripts

| Script | What it does |
|--------|-------------|
| `bin/build-clean.ps1` | Clean solution + remove `out/` and `artifacts/` |
| `bin/build-rebuild.ps1` | Force rebuild in Release |
| `bin/build-test.ps1` | Build + run all tests |
| `bin/build-test-codecoverage.ps1` | Build + test + HTML coverage report |

## Rules

1. **Never edit files in `Generated/` folders** — they will be overwritten on next scaffold
2. **Extend via partial classes** in the parent folder (e.g. `Alerts/`)
3. **Lookup data lives in database tables** — no C# enums for reference data
4. **The scaffold script is the entry point** — not raw `dotnet ef` commands
5. **Tests use SQLite in-memory** — no SQL Server dependency for unit tests
