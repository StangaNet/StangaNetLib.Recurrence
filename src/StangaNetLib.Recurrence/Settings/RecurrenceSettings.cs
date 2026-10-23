namespace StangaNetLib.Recurrence.Settings;

/// <summary>Configuration for recurrence materialization.</summary>
public sealed class RecurrenceSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "RecurrenceSettings";

    /// <summary>How many days ahead of "today" (per rule timezone) to materialize. Default 60.</summary>
    public int HorizonDays { get; set; } = 60;
}
