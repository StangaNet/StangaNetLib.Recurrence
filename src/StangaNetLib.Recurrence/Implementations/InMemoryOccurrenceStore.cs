using System.Collections.Concurrent;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Guards;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Implementations;

/// <summary>Thread-safe in-memory <see cref="IOccurrenceStore"/> for tests and local development.</summary>
public sealed class InMemoryOccurrenceStore : IOccurrenceStore
{
    private readonly ConcurrentDictionary<string, Occurrence> _items =
        new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<Occurrence?> GetByNaturalKeyAsync(
        string naturalKey,
        CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(naturalKey, out var item);
        return Task.FromResult(item);
    }

    /// <inheritdoc />
    public Task<Result> SaveAsync(Occurrence occurrence, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(occurrence, nameof(occurrence));
        Guard.Against.NullOrWhiteSpace(occurrence.NaturalKey, nameof(occurrence.NaturalKey));
        _items[occurrence.NaturalKey] = occurrence;
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Occurrence>> ListInRangeAsync(
        DateOnly from,
        DateOnly to,
        string? ruleId = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Occurrence> list = _items.Values
            .Where(o => o.DueDate >= from && o.DueDate <= to)
            .Where(o => ruleId is null || o.RuleId == ruleId)
            .OrderBy(o => o.DueDate)
            .ToList();

        return Task.FromResult(list);
    }
}
