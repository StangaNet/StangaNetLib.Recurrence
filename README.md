# StangaNetLib.Recurrence

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Recurrence/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Recurrence/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)

Recurring **calendar rules** and **idempotent occurrence materialization**. This library decides *what* to create and *when*; periodic execution belongs to [`StangaNetLib.Jobs`](https://github.com/StangaNet/StangaNetLib.Jobs) (or your host).

## Installation

```xml
<PackageReference Include="StangaNetLib.Recurrence" Version="1.0.0" />
```

## Core Components

| Namespace | Components | Purpose |
|---|---|---|
| `Abstractions` | `IRecurrenceMaterializer`, `IRecurrenceRuleStore`, `IOccurrenceStore` | Materialize + persistence ports |
| `Models` | `RecurrenceRule`, `Occurrence`, frequencies/status | Domain models |
| `Extensions` | `AddStangaNetLibRecurrence`, `AddStangaNetLibRecurrenceInMemory` | DI |

## Date policy

Rules use **DateOnly** values. "Today" and the materialization horizon are computed in the rule's `TimeZoneId` (default `UTC`). Monthly `DayOfMonth` values that exceed a month's length are **clamped** to the last day of that month.

## Quick Start

```csharp
builder.Services.AddStangaNetLibRecurrenceInMemory(builder.Configuration);
// Optional: builder.Services.AddStangaNetLibConcurrency(); // keyed lock for MaterializeAsync

var rules = sp.GetRequiredService<IRecurrenceRuleStore>();
await rules.SaveAsync(new RecurrenceRule
{
    Id = "rent",
    Frequency = RecurrenceFrequency.Monthly,
    Interval = 1,
    DayOfMonth = 31,
    Start = new DateOnly(2026, 1, 31),
    TimeZoneId = "UTC"
});

var materializer = sp.GetRequiredService<IRecurrenceMaterializer>();
await materializer.MaterializeAsync(); // up to HorizonDays (default 60)
```

Deactivating a rule stops future materialization; existing occurrences remain.

## License

Apache-2.0. See [LICENSE](./LICENSE).
