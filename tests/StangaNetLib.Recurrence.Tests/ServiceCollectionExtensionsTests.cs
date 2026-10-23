using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Extensions;

namespace StangaNetLib.Recurrence.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStangaNetLibRecurrenceInMemory_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibRecurrenceInMemory();
        var sp = services.BuildServiceProvider();

        sp.GetService<IRecurrenceMaterializer>().Should().NotBeNull();
        sp.GetService<IRecurrenceRuleStore>().Should().NotBeNull();
        sp.GetService<IOccurrenceStore>().Should().NotBeNull();
    }
}
