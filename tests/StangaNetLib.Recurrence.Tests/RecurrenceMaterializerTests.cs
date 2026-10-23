using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.Recurrence.Abstractions;
using StangaNetLib.Recurrence.Extensions;
using StangaNetLib.Recurrence.Models;

namespace StangaNetLib.Recurrence.Tests;

public class RecurrenceMaterializerTests
{
    private static (IRecurrenceMaterializer Materializer, IRecurrenceRuleStore Rules, IOccurrenceStore Occurrences)
        CreateSut(int horizonDays = 60)
    {
        var services = new ServiceCollection();
        services.AddStangaNetLibRecurrenceInMemory();
        services.Configure<StangaNetLib.Recurrence.Settings.RecurrenceSettings>(s => s.HorizonDays = horizonDays);
        var sp = services.BuildServiceProvider();
        return (
            sp.GetRequiredService<IRecurrenceMaterializer>(),
            sp.GetRequiredService<IRecurrenceRuleStore>(),
            sp.GetRequiredService<IOccurrenceStore>());
    }

    [Fact]
    public async Task Materialize_Monthly_ClampsEndOfMonth()
    {
        var (materializer, rules, occurrences) = CreateSut(horizonDays: 90);
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "eom",
            Frequency = RecurrenceFrequency.Monthly,
            Interval = 1,
            DayOfMonth = 31,
            Start = new DateOnly(2026, 1, 31),
            End = new DateOnly(2026, 4, 30),
            TimeZoneId = "UTC"
        });

        var result = await materializer.MaterializeAsync(horizonDays: 365);
        result.IsSuccess.Should().BeTrue();

        var list = await occurrences.ListInRangeAsync(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 30), "eom");

        list.Select(o => o.DueDate).Should().Equal(
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 28),
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 4, 30));
    }

    [Fact]
    public async Task Materialize_Twice_IsIdempotent()
    {
        var (materializer, rules, _) = CreateSut();
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "daily",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            TimeZoneId = "UTC"
        });

        var first = await materializer.MaterializeAsync();
        var second = await materializer.MaterializeAsync();

        first.IsSuccess.Should().BeTrue();
        first.Value.Should().BeGreaterThan(0);
        second.IsSuccess.Should().BeTrue();
        second.Value.Should().Be(0);
    }

    [Fact]
    public async Task Deactivate_StopsFutureMaterialization()
    {
        var (materializer, rules, occurrences) = CreateSut(horizonDays: 5);
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "x",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = start,
            TimeZoneId = "UTC"
        });

        await materializer.MaterializeAsync();
        var before = (await occurrences.ListInRangeAsync(start, start.AddDays(30), "x")).Count;

        await materializer.DeactivateRuleAsync("x");
        // Extend end into the future by saving a longer-active clone would not run — rule inactive
        var afterMaterialize = await materializer.MaterializeAsync(horizonDays: 30);
        afterMaterialize.Value.Should().Be(0);

        var after = (await occurrences.ListInRangeAsync(start, start.AddDays(30), "x")).Count;
        after.Should().Be(before);
    }

    [Fact]
    public async Task SkipOccurrence_MarksSkipped()
    {
        var (materializer, rules, occurrences) = CreateSut();
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "skip",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = start,
            TimeZoneId = "UTC"
        });

        await materializer.MaterializeAsync();
        var key = Occurrence.BuildNaturalKey("skip", start);
        var skip = await materializer.SkipOccurrenceAsync(key);
        skip.IsSuccess.Should().BeTrue();

        var occ = await occurrences.GetByNaturalKeyAsync(key);
        occ!.Status.Should().Be(OccurrenceStatus.Skipped);
    }

    [Fact]
    public async Task GetOccurrencesInRange_FiltersByRule()
    {
        var (materializer, rules, _) = CreateSut();
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "a",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = start,
            TimeZoneId = "UTC"
        });
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "b",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = start,
            TimeZoneId = "UTC"
        });

        await materializer.MaterializeAsync();
        var range = await materializer.GetOccurrencesInRangeAsync(start, start.AddDays(10), "a");
        range.IsSuccess.Should().BeTrue();
        range.Value!.Should().OnlyContain(o => o.RuleId == "a");
    }

    [Fact]
    public async Task GetNext_ReturnsEarliestPlanned()
    {
        var (materializer, rules, _) = CreateSut();
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "next",
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            Start = start,
            TimeZoneId = "UTC"
        });
        await materializer.MaterializeAsync();
        await materializer.SkipOccurrenceAsync(Occurrence.BuildNaturalKey("next", start));

        var next = await materializer.GetNextAsync("next", start);
        next.IsSuccess.Should().BeTrue();
        next.Value!.DueDate.Should().Be(start.AddDays(1));
    }

    [Fact]
    public async Task Interval_Weekly_RespectsStep()
    {
        var (materializer, rules, occurrences) = CreateSut(horizonDays: 21);
        var start = new DateOnly(2026, 1, 5); // Monday
        await rules.SaveAsync(new RecurrenceRule
        {
            Id = "w",
            Frequency = RecurrenceFrequency.Weekly,
            Interval = 2,
            Start = start,
            End = start.AddDays(28),
            TimeZoneId = "UTC"
        });

        await materializer.MaterializeAsync(horizonDays: 365);
        var list = await occurrences.ListInRangeAsync(start, start.AddDays(28), "w");
        list.Select(o => o.DueDate).Should().Equal(
            start,
            start.AddDays(14),
            start.AddDays(28));
    }
}
