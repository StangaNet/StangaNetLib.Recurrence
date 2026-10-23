using StangaNetLib.Core.Common;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Abstractions;

/// <summary>Persistence port for recurrence rules (implemented by the consumer).</summary>
public interface IRecurrenceRuleStore
{
    /// <summary>Gets a rule by id, or null when missing.</summary>
    Task<RecurrenceRule?> GetAsync(string ruleId, CancellationToken cancellationToken = default);

    /// <summary>Lists all currently active rules.</summary>
    Task<IReadOnlyList<RecurrenceRule>> ListActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Inserts or updates a rule.</summary>
    Task<Result> SaveAsync(RecurrenceRule rule, CancellationToken cancellationToken = default);
}
