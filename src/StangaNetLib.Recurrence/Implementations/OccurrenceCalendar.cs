using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Implementations;

/// <summary>Pure calendar expansion for recurrence rules (date-only).</summary>
internal static class OccurrenceCalendar
{
    /// <summary>
    /// Yields occurrence dates from <paramref name="rule"/>.Start up to and including
    /// <paramref name="horizonEnd"/>, respecting End and Interval.
    /// </summary>
    public static IEnumerable<DateOnly> EnumerateDueDates(RecurrenceRule rule, DateOnly horizonEnd)
    {
        if (rule.Interval < 1)
            yield break;

        var hardEnd = rule.End is { } end && end < horizonEnd ? end : horizonEnd;
        if (rule.Start > hardEnd)
            yield break;

        switch (rule.Frequency)
        {
            case RecurrenceFrequency.Daily:
                for (var d = rule.Start; d <= hardEnd; d = d.AddDays(rule.Interval))
                    yield return d;
                break;

            case RecurrenceFrequency.Weekly:
                for (var d = rule.Start; d <= hardEnd; d = d.AddDays(7 * rule.Interval))
                    yield return d;
                break;

            case RecurrenceFrequency.Monthly:
            {
                var day = rule.DayOfMonth ?? rule.Start.Day;
                var cursor = new DateOnly(rule.Start.Year, rule.Start.Month, 1);
                // Align first candidate to start month
                var first = ClampDay(cursor.Year, cursor.Month, day);
                if (first < rule.Start)
                    cursor = cursor.AddMonths(rule.Interval);

                for (; ; cursor = cursor.AddMonths(rule.Interval))
                {
                    var due = ClampDay(cursor.Year, cursor.Month, day);
                    if (due < rule.Start)
                        continue;
                    if (due > hardEnd)
                        yield break;
                    yield return due;
                }
            }

            case RecurrenceFrequency.Yearly:
            {
                var month = rule.Start.Month;
                var day = rule.DayOfMonth ?? rule.Start.Day;
                for (var year = rule.Start.Year; ; year += rule.Interval)
                {
                    var due = ClampDay(year, month, day);
                    if (due < rule.Start)
                        continue;
                    if (due > hardEnd)
                        yield break;
                    yield return due;
                }
            }

            default:
                yield break;
        }
    }

    internal static DateOnly TodayInTimeZone(string timeZoneId)
    {
        var tz = ResolveTimeZone(timeZoneId);
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
        return DateOnly.FromDateTime(local.DateTime);
    }

    internal static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Common alias when Windows id is unavailable
            if (string.Equals(timeZoneId, "UTC", StringComparison.OrdinalIgnoreCase))
                return TimeZoneInfo.Utc;
            throw;
        }
        catch (InvalidTimeZoneException)
        {
            if (string.Equals(timeZoneId, "UTC", StringComparison.OrdinalIgnoreCase))
                return TimeZoneInfo.Utc;
            throw;
        }
    }

    private static DateOnly ClampDay(int year, int month, int day)
    {
        var last = DateTime.DaysInMonth(year, month);
        return new DateOnly(year, month, Math.Min(day, last));
    }
}
