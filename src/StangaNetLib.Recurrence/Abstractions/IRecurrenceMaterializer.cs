using StangaNetLib.Core.Common;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Abstractions;

/// <summary>
/// Materializes recurrence occurrences up to a horizon and exposes query/skip/deactivate APIs.
/// This is not a job runner — pair with <c>StangaNetLib.Jobs</c> (or the host) for periodic calls.
/// </summary>
public interface IRecurrenceMaterializer
{
    /// <summary>
    /// Materializes missing occurrences for all active rules up to today+horizon (per rule timezone).
    /// Idempotent: existing natural keys are not duplicated.
    /// </summary>
    /// <param name="horizonDays">Override of settings default; null uses configured horizon.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of newly created occurrences.</returns>
    Task<Result<int>> MaterializeAsync(int? horizonDays = null, CancellationToken cancellationToken = default);

    /// <summary>Returns the next planned occurrence on or after <paramref name="from"/> (default: today in rule TZ).</summary>
    Task<Result<Occurrence?>> GetNextAsync(
        string ruleId,
        DateOnly? from = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists stored occurrences in an inclusive date range.</summary>
    Task<Result<IReadOnlyList<Occurrence>>> GetOccurrencesInRangeAsync(
        DateOnly from,
        DateOnly to,
        string? ruleId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Marks a single occurrence as skipped (does not delete it).</summary>
    Task<Result> SkipOccurrenceAsync(string naturalKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a rule so future materializations stop.
    /// Already materialized occurrences are left unchanged.
    /// </summary>
    Task<Result> DeactivateRuleAsync(string ruleId, CancellationToken cancellationToken = default);
}
