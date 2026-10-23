using System.Collections.Concurrent;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Guards;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Implementations;

/// <summary>Thread-safe in-memory <see cref="IRecurrenceRuleStore"/> for tests and local development.</summary>
public sealed class InMemoryRecurrenceRuleStore : IRecurrenceRuleStore
{
    private readonly ConcurrentDictionary<string, RecurrenceRule> _rules =
        new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<RecurrenceRule?> GetAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        _rules.TryGetValue(ruleId, out var rule);
        return Task.FromResult(rule);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RecurrenceRule>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RecurrenceRule> list = _rules.Values.Where(r => r.IsActive).ToList();
        return Task.FromResult(list);
    }

    /// <inheritdoc />
    public Task<Result> SaveAsync(RecurrenceRule rule, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(rule, nameof(rule));
        Guard.Against.NullOrWhiteSpace(rule.Id, nameof(rule.Id));
        _rules[rule.Id] = rule;
        return Task.FromResult(Result.Success());
    }
}
