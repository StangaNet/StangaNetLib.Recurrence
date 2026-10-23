using StangaNetLib.Core.Common;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Abstractions;

/// <summary>Persistence port for materialized occurrences (implemented by the consumer).</summary>
public interface IOccurrenceStore
{
    /// <summary>Gets an occurrence by natural key, or null when missing.</summary>
    Task<Occurrence?> GetByNaturalKeyAsync(string naturalKey, CancellationToken cancellationToken = default);

    /// <summary>Inserts or updates an occurrence.</summary>
    Task<Result> SaveAsync(Occurrence occurrence, CancellationToken cancellationToken = default);

    /// <summary>Lists occurrences for a rule (optional) within an inclusive date range.</summary>
    Task<IReadOnlyList<Occurrence>> ListInRangeAsync(
        DateOnly from,
        DateOnly to,
        string? ruleId = null,
        CancellationToken cancellationToken = default);
}
