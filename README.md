# KoreForge.Data

Database-first EF Core data layer for KoreForge applications.

## Overview

KoreForge.Data provides scaffolded EF Core `DbContext` and entity classes generated from existing SQL Server databases. The library follows a database-first approach — the database schema is the source of truth, and all entity code is produced by `dotnet ef dbcontext scaffold`.

## Design Principles

1. **Database-first** — schema is authoritative; EF migrations are not used
2. **Generated code is disposable** — scaffold output can be deleted and recreated at any time
3. **Custom code survives regeneration** — extensions live in partial classes outside the Generated/ folder
4. **Scripts are the entry point** — developers use PowerShell scripts, not raw CLI commands
5. **Lookup tables, not enums** — reference data lives in database tables with FK relationships

## Package

| Package | NuGet |
|---------|-------|
| `KoreForge.Data` | [![NuGet](https://img.shields.io/nuget/v/KoreForge.Data)](https://www.nuget.org/packages/KoreForge.Data) |

## Project Structure

```text
KoreForge.Data/
├── config/
│   └── scaffold-config.json          # Drives scaffolding — one entry per database
├── scr/
│   ├── scaffold-db.ps1               # Config-driven scaffold runner
│   └── AlertsDB-Notification-Schema.sql  # DDL for the Notification schema
├── src/KF.Data/
│   ├── AlertsDbContext.cs             # Partial context extension  (ns: KF.Data)
│   ├── AlertsDbOptions.cs             # Connection options          (ns: KF.Data)
│   ├── AlertsDbServiceCollectionExtensions.cs  # DI registration   (ns: KF.Data)
│   └── Generated/                     # Scaffold output — DO NOT EDIT
│       └── Alerts/                    # AlertsDB database
│           ├── AlertsDbContext.cs      # Generated context          (ns: KF.Data)
│           └── Notification/          # Notification schema entities
│               ├── Channel.cs         # (ns: KF.Data.Alerts.Notification)
│               ├── Priority.cs
│               ├── OutboxStatus.cs
│               ├── SendOutcome.cs
│               ├── NotificationOutbox.cs
│               ├── EmailPayload.cs
│               └── SmsPayload.cs
├── tst/KF.Data.Tests/
├── scr/                              # Build & release scripts
├── doc/                              # Documentation
└── artifacts/                        # NuGet package output
```

## Quick Start

### 1. Install

```shell
dotnet add package KoreForge.Data
```

### 2. Register in `Program.cs`

```csharp
using KF.Data;

builder.Services.AddAlertsDb(opts =>
    opts.ConnectionString = builder.Configuration.GetConnectionString("AlertsDB")!);
```

Or with a raw connection string:

```csharp
builder.Services.AddAlertsDb("Server=.;Database=AlertsDB;...";
```

### 3. Inject and Use

```csharp
using KF.Data.Alerts.Notification;

public class NotificationService(AlertsDbContext db)
{
    public async Task<List<NotificationOutbox>> GetPendingAsync(CancellationToken ct)
    {
        var pendingStatus = await db.OutboxStatus
            .SingleAsync(s => s.Name == "Pending", ct);

        return await db.NotificationOutbox
            .Include(n => n.Channel)
            .Include(n => n.Priority)
            .Where(n => n.OutboxStatusId == pendingStatus.OutboxStatusId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(ct);
    }
}
```

## Databases

### AlertsDB — Notification Schema

| Table | Purpose |
|-------|---------|
| `Channel` | Lookup: Email, SMS, Push, InApp |
| `Priority` | Lookup: Low, Normal, High, Critical |
| `OutboxStatus` | Lookup: Pending, Processing, Sent, Failed, Cancelled |
| `SendOutcome` | Lookup: Success, HardBounce, SoftBounce, Rejected, Timeout, ProviderError |
| `NotificationOutbox` | Core outbox table with FK to all lookups |
| `EmailPayload` | Email-specific fields (from, cc, bcc, html flag) |
| `SmsPayload` | SMS-specific fields (from number, provider message id) |

All lookup values are database rows — no C# enums. FK relationships provide navigation properties automatically via scaffold.

## Scaffolding

### Prerequisites

- .NET 10 SDK
- SQL Server instance with the target database
- Run `dotnet tool restore` once to install `dotnet-ef`

### Running the Scaffold

```shell
.\scr\scaffold-db.ps1
```

This reads `config/scaffold-config.json`, cleans existing generated output, and runs `dotnet ef dbcontext scaffold` for each configured database.

To scaffold a single database:

```shell
.\scr\scaffold-db.ps1 -Database AlertsDB
```

### Adding a New Database

1. Run the SQL DDL against the target server
2. Add a new entry to `config/scaffold-config.json`
3. Run `.\scr\scaffold-db.ps1 -Database NewDbName`
4. Add options class, DI registration, and empty partial context at `src/KF.Data/` project root
5. Set `namespace` and `contextNamespace` in the config to keep "Generated" out of namespaces

### Extending Generated Code

Generated files live in `Generated/` and must never be edited by hand. To add custom behaviour, create partial classes at the project root (or any folder outside `Generated/`):

```csharp
// src/KF.Data/NotificationOutboxExtensions.cs
namespace KF.Data.Alerts.Notification;

public partial class NotificationOutbox
{
    public bool IsOverdue => OutboxStatus?.Name == "Pending"
        && CreatedAt < DateTimeOffset.UtcNow.AddHours(-1);
}
```

An empty partial `AlertsDbContext` is provided at `src/KF.Data/AlertsDbContext.cs` for context-level extensions.

### Namespace Convention

| Item | Namespace | Example |
|------|----------|--------|
| DbContext, Options, DI | `{RootNs}` | `KF.Data` |
| Entity models | `{RootNs}.{DbName}.{Schema}` | `KF.Data.Alerts.Notification` |
| Entity models (dbo schema) | `{RootNs}.{DbName}` | `KF.Data.Alerts` |

"Generated" never appears in a namespace — it is purely a folder for scaffold output.

## Scripts

| Script | Purpose |
|--------|---------|
| `scr/scaffold-db.ps1` | Config-driven scaffold runner |
| `scr/build-clean.ps1` | Clean build outputs |
| `scr/build-rebuild.ps1` | Force rebuild |
| `scr/build-test.ps1` | Build + run tests |
| `scr/build-test-codecoverage.ps1` | Build + test + coverage report |
| `scr/git-push.ps1` | Add, commit, push |
| `scr/git-push-nuget.ps1` | Tag and push for NuGet release |

## License

[MIT](LICENSE.md)


