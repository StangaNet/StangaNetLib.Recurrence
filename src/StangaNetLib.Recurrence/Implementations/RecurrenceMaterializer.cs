using Microsoft.Extensions.Options;
using StangaNetLib.Concurrency.Locking;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Guards;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Errors;
using StangaNetLib.Recurrence.Models;
using StangaNetLib.Recurrence.Settings;

namespace StangaNetLib.Recurrence.Implementations;

/// <summary>Default <see cref="IRecurrenceMaterializer"/> implementation.</summary>
public sealed class RecurrenceMaterializer : IRecurrenceMaterializer
{
    private readonly IRecurrenceRuleStore _ruleStore;
    private readonly IOccurrenceStore _occurrenceStore;
    private readonly RecurrenceSettings _settings;
    private readonly IKeyedLock? _keyedLock;

    /// <summary>Creates the materializer.</summary>
    public RecurrenceMaterializer(
        IRecurrenceRuleStore ruleStore,
        IOccurrenceStore occurrenceStore,
        IOptions<RecurrenceSettings> settings,
        IKeyedLock? keyedLock = null)
    {
        _ruleStore = Guard.Against.Null(ruleStore, nameof(ruleStore));
        _occurrenceStore = Guard.Against.Null(occurrenceStore, nameof(occurrenceStore));
        Guard.Against.Null(settings, nameof(settings));
        _settings = settings.Value;
        _keyedLock = keyedLock;
    }

    /// <inheritdoc />
    public async Task<Result<int>> MaterializeAsync(
        int? horizonDays = null,
        CancellationToken cancellationToken = default)
    {
        var horizon = horizonDays ?? _settings.HorizonDays;
        if (horizon < 0)
            return Result<int>.Failure(RecurrenceErrors.InvalidHorizon());

        var rules = await _ruleStore.ListActiveAsync(cancellationToken).ConfigureAwait(false);
        var created = 0;

        foreach (var rule in rules)
        {
            if (rule.Interval < 1)
                return Result<int>.Failure(RecurrenceErrors.InvalidInterval());

            TimeZoneInfo tz;
            try
            {
                tz = OccurrenceCalendar.ResolveTimeZone(rule.TimeZoneId);
            }
            catch (Exception)
            {
                return Result<int>.Failure(RecurrenceErrors.InvalidTimeZone(rule.TimeZoneId));
            }

            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).DateTime);
            var horizonEnd = today.AddDays(horizon);

            if (_keyedLock is not null)
            {
                await using var lease = await _keyedLock
                    .AcquireAsync($"recurrence:materialize:{rule.Id}", cancellationToken)
                    .ConfigureAwait(false);
                created += await MaterializeRuleAsync(rule, horizonEnd, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                created += await MaterializeRuleAsync(rule, horizonEnd, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result<int>.Success(created);
    }

    /// <inheritdoc />
    public async Task<Result<Occurrence?>> GetNextAsync(
        string ruleId,
        DateOnly? from = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(ruleId, nameof(ruleId));

        var rule = await _ruleStore.GetAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule is null)
            return Result<Occurrence?>.Failure(RecurrenceErrors.RuleNotFound(ruleId));

        DateOnly startFrom;
        try
        {
            startFrom = from ?? OccurrenceCalendar.TodayInTimeZone(rule.TimeZoneId);
        }
        catch (Exception)
        {
            return Result<Occurrence?>.Failure(RecurrenceErrors.InvalidTimeZone(rule.TimeZoneId));
        }

        var rangeEnd = rule.End ?? startFrom.AddYears(10);
        var list = await _occurrenceStore
            .ListInRangeAsync(startFrom, rangeEnd, ruleId, cancellationToken)
            .ConfigureAwait(false);

        var next = list
            .Where(o => o.Status == OccurrenceStatus.Planned && o.DueDate >= startFrom)
            .OrderBy(o => o.DueDate)
            .FirstOrDefault();

        return Result<Occurrence?>.Success(next);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Occurrence>>> GetOccurrencesInRangeAsync(
        DateOnly from,
        DateOnly to,
        string? ruleId = null,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
            return Result<IReadOnlyList<Occurrence>>.Failure(RecurrenceErrors.InvalidRange());

        var list = await _occurrenceStore
            .ListInRangeAsync(from, to, ruleId, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<Occurrence>>.Success(list);
    }

    /// <inheritdoc />
    public async Task<Result> SkipOccurrenceAsync(
        string naturalKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(naturalKey, nameof(naturalKey));

        var occurrence = await _occurrenceStore
            .GetByNaturalKeyAsync(naturalKey, cancellationToken)
            .ConfigureAwait(false);

        if (occurrence is null)
            return Result.Failure(RecurrenceErrors.OccurrenceNotFound(naturalKey));

        occurrence.Status = OccurrenceStatus.Skipped;
        return await _occurrenceStore.SaveAsync(occurrence, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result> DeactivateRuleAsync(
        string ruleId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(ruleId, nameof(ruleId));

        var rule = await _ruleStore.GetAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule is null)
            return Result.Failure(RecurrenceErrors.RuleNotFound(ruleId));

        rule.IsActive = false;
        return await _ruleStore.SaveAsync(rule, cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> MaterializeRuleAsync(
        RecurrenceRule rule,
        DateOnly horizonEnd,
        CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var due in OccurrenceCalendar.EnumerateDueDates(rule, horizonEnd))
        {
            var naturalKey = Occurrence.BuildNaturalKey(rule.Id, due);
            var existing = await _occurrenceStore
                .GetByNaturalKeyAsync(naturalKey, cancellationToken)
                .ConfigureAwait(false);

            if (existing is not null)
                continue;

            var occurrence = new Occurrence
            {
                Id = naturalKey,
                RuleId = rule.Id,
                DueDate = due,
                Status = OccurrenceStatus.Planned,
                NaturalKey = naturalKey
            };

            var save = await _occurrenceStore.SaveAsync(occurrence, cancellationToken).ConfigureAwait(false);
            if (save.IsFailure)
                continue;

            created++;
        }

        return created;
    }
}
