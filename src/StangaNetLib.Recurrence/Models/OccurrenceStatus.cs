namespace StangaNetLib.Recurrence.Models;

/// <summary>Lifecycle state of a materialized occurrence.</summary>
public enum OccurrenceStatus
{
    /// <summary>Scheduled / pending consumer action.</summary>
    Planned = 0,

    /// <summary>Explicitly skipped; remains stored for idempotency.</summary>
    Skipped = 1
}
