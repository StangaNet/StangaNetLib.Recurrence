namespace StangaNetLib.Recurrence.Models;

/// <summary>
/// A recurring calendar rule. Operates on <see cref="DateOnly"/> values interpreted in
/// <see cref="TimeZoneId"/> (default <c>UTC</c>).
/// </summary>
public sealed class RecurrenceRule
{
    /// <summary>Stable rule identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Frequency of recurrence.</summary>
    public RecurrenceFrequency Frequency { get; init; }

    /// <summary>Step between occurrences (≥ 1).</summary>
    public int Interval { get; init; } = 1;

    /// <summary>
    /// Day of month for <see cref="RecurrenceFrequency.Monthly"/> (1–31).
    /// When the month is shorter, the last day of that month is used.
    /// </summary>
    public int? DayOfMonth { get; init; }

    /// <summary>First eligible occurrence date (inclusive).</summary>
    public DateOnly Start { get; init; }

    /// <summary>Optional last eligible occurrence date (inclusive).</summary>
    public DateOnly? End { get; init; }

    /// <summary>
    /// IANA or Windows time-zone id used to compute "today" for horizon materialization.
    /// Defaults to <c>UTC</c>.
    /// </summary>
    public string TimeZoneId { get; init; } = "UTC";

    /// <summary>When false, materialization stops creating future occurrences.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional opaque consumer payload (not interpreted by this library).</summary>
    public string? Metadata { get; init; }
}
