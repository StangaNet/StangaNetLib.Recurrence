using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StangaNetLib.Concurrency.Locking;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Implementations;
using StangaNetLib.Recurrence.Settings;

namespace StangaNetLib.Recurrence.Extensions;

/// <summary>DI registration for StangaNetLib.Recurrence.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the materializer and binds <see cref="RecurrenceSettings"/>.
    /// The consumer must also register <see cref="IRecurrenceRuleStore"/> and <see cref="IOccurrenceStore"/>.
    /// Optionally register <see cref="IKeyedLock"/> (via Concurrency) for in-process materialize locking.
    /// </summary>
    public static IServiceCollection AddStangaNetLibRecurrence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RecurrenceSettings>(configuration.GetSection(RecurrenceSettings.SectionName));
        return RegisterCore(services);
    }

    /// <summary>
    /// Registers the materializer with default settings.
    /// The consumer must register rule/occurrence stores.
    /// </summary>
    public static IServiceCollection AddStangaNetLibRecurrence(this IServiceCollection services)
    {
        services.Configure<RecurrenceSettings>(_ => { });
        return RegisterCore(services);
    }

    /// <summary>
    /// Registers materializer plus in-memory rule and occurrence stores (tests / local dev).
    /// </summary>
    public static IServiceCollection AddStangaNetLibRecurrenceInMemory(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        if (configuration is not null)
            services.Configure<RecurrenceSettings>(configuration.GetSection(RecurrenceSettings.SectionName));
        else
            services.Configure<RecurrenceSettings>(_ => { });

        services.AddSingleton<IValidateOptions<RecurrenceSettings>, RecurrenceSettingsValidator>();
        services.TryAddSingleton<IRecurrenceRuleStore, InMemoryRecurrenceRuleStore>();
        services.TryAddSingleton<IOccurrenceStore, InMemoryOccurrenceStore>();
        services.TryAddSingleton<IRecurrenceMaterializer>(sp =>
            new RecurrenceMaterializer(
                sp.GetRequiredService<IRecurrenceRuleStore>(),
                sp.GetRequiredService<IOccurrenceStore>(),
                sp.GetRequiredService<IOptions<RecurrenceSettings>>(),
                sp.GetService<IKeyedLock>()));

        return services;
    }

    private static IServiceCollection RegisterCore(IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<RecurrenceSettings>, RecurrenceSettingsValidator>();
        services.TryAddSingleton<IRecurrenceMaterializer>(sp =>
            new RecurrenceMaterializer(
                sp.GetRequiredService<IRecurrenceRuleStore>(),
                sp.GetRequiredService<IOccurrenceStore>(),
                sp.GetRequiredService<IOptions<RecurrenceSettings>>(),
                sp.GetService<IKeyedLock>()));

        return services;
    }
}
