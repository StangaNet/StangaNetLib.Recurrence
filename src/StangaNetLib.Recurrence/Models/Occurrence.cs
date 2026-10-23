namespace StangaNetLib.Recurrence.Models;

/// <summary>A single materialized occurrence of a <see cref="RecurrenceRule"/>.</summary>
public sealed class Occurrence
{
    /// <summary>Stable occurrence identifier (may equal <see cref="NaturalKey"/>).</summary>
    public required string Id { get; init; }

    /// <summary>Parent rule id.</summary>
    public required string RuleId { get; init; }

    /// <summary>Calendar due date (date-only in the rule timezone).</summary>
    public DateOnly DueDate { get; init; }

    /// <summary>Current status.</summary>
    public OccurrenceStatus Status { get; set; } = OccurrenceStatus.Planned;

    /// <summary>
    /// Anti-duplicate key: <c>{ruleId}:{due:yyyy-MM-dd}</c>.
    /// </summary>
    public required string NaturalKey { get; init; }

    /// <summary>Builds the canonical natural key for a rule/date pair.</summary>
    public static string BuildNaturalKey(string ruleId, DateOnly dueDate) =>
        $"{ruleId}:{dueDate:yyyy-MM-dd}";
}
