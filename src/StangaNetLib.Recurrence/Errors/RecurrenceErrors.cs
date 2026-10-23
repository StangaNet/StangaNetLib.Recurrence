using StangaNetLib.Core.Common;

namespace StangaNetLib.Recurrence.Errors;

internal static class RecurrenceErrors
{
    internal static Error RuleNotFound(string ruleId)
        => Error.NotFound("RecurrenceRule", ruleId);

    internal static Error OccurrenceNotFound(string naturalKey)
        => Error.NotFound("Occurrence", naturalKey);

    internal static Error InvalidInterval()
        => Error.Validation("Interval", "Interval must be greater than or equal to 1.");

    internal static Error InvalidHorizon()
        => Error.Validation("HorizonDays", "Horizon days must be greater than or equal to 0.");

    internal static Error InvalidRange()
        => Error.Validation("Range", "'from' must be less than or equal to 'to'.");

    internal static Error InvalidTimeZone(string timeZoneId)
        => Error.Validation("TimeZoneId", $"Unknown time zone '{timeZoneId}'.");
}
