# Usage Guide

## Installation

```shell
dotnet add package KoreForge.Data
```

## Registering a Database Context

KoreForge.Data provides a DI extension method for each scaffolded database. For AlertsDB:

```csharp
using KF.Data.Alerts;

builder.Services.AddAlertsDb(opts =>
    opts.ConnectionString = builder.Configuration.GetConnectionString("AlertsDB")!);
```

This registers `AlertsDbContext` as a scoped service with SQL Server configured.

## Querying Entities

All entities are in the `KF.Data.Generated.Alerts` namespace, and contexts are in `KF.Data.Generated`:

```csharp
using KF.Data.Generated;
using KF.Data.Generated.Alerts;

// Get all channels
var channels = await db.Channel.ToListAsync(ct);

// Get pending notifications with navigation properties
var pending = await db.NotificationOutbox
    .Include(n => n.Channel)
    .Include(n => n.Priority)
    .Include(n => n.OutboxStatus)
    .Where(n => n.OutboxStatus.Name == "Pending")
    .ToListAsync(ct);
```

## Lookup Tables

Reference data is stored in database lookup tables, not C# enums. Each lookup table has:

- An identity primary key (e.g. `ChannelId`)
- A `Name` column with a unique constraint
- A reverse navigation collection on the lookup entity

Query lookup values by name:

```csharp
var emailChannel = await db.Channel.SingleAsync(c => c.Name == "Email", ct);
```

FK navigation properties on `NotificationOutbox` allow direct traversal:

```csharp
var notification = await db.NotificationOutbox
    .Include(n => n.Channel)
    .FirstAsync(ct);

Console.WriteLine(notification.Channel.Name); // "Email"
```

## Working with Payloads

Each notification can have channel-specific payload data:

```csharp
// Email payload
var emailNotifications = await db.NotificationOutbox
    .Include(n => n.EmailPayload)
    .Where(n => n.Channel.Name == "Email")
    .ToListAsync(ct);

foreach (var n in emailNotifications)
{
    foreach (var email in n.EmailPayload)
    {
        Console.WriteLine($"From: {email.FromAddress}, HTML: {email.IsHtml}");
    }
}

// SMS payload
var smsNotifications = await db.NotificationOutbox
    .Include(n => n.SmsPayload)
    .Where(n => n.Channel.Name == "SMS")
    .ToListAsync(ct);
```

## Extending Entities

To add computed properties or methods, create partial classes **outside** the `Generated/` folder:

```csharp
// src/KF.Data/Alerts/NotificationOutboxExtensions.cs
namespace KF.Data.Generated.Alerts;

public partial class NotificationOutbox
{
    public bool IsOverdue => OutboxStatus?.Name == "Pending"
        && CreatedAt < DateTimeOffset.UtcNow.AddHours(-1);
}
```

To extend the context, use the provided partial at `src/KF.Data/Alerts/AlertsDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace KF.Data.Generated;

public partial class AlertsDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Additional model configuration
    }
}
```

These survive scaffold regeneration because they are in separate files outside `Generated/`.
