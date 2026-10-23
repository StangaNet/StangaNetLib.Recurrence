using Microsoft.Extensions.Options;

namespace StangaNetLib.Recurrence.Settings;

/// <summary>Validates <see cref="RecurrenceSettings"/>.</summary>
public sealed class RecurrenceSettingsValidator : IValidateOptions<RecurrenceSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RecurrenceSettings options)
    {
        if (options.HorizonDays < 0)
            return ValidateOptionsResult.Fail("HorizonDays must be greater than or equal to 0.");

        return ValidateOptionsResult.Success;
    }
}
