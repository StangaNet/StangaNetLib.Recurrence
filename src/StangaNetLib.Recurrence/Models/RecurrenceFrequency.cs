namespace StangaNetLib.Recurrence.Models;

/// <summary>How often a recurrence rule fires.</summary>
public enum RecurrenceFrequency
{
    /// <summary>Every <c>Interval</c> days.</summary>
    Daily = 0,

    /// <summary>Every <c>Interval</c> weeks from the start date's weekday.</summary>
    Weekly = 1,

    /// <summary>Every <c>Interval</c> months on <see cref="RecurrenceRule.DayOfMonth"/> (clamped to month length).</summary>
    Monthly = 2,

    /// <summary>Every <c>Interval</c> years on the start month/day (day clamped).</summary>
    Yearly = 3
}
