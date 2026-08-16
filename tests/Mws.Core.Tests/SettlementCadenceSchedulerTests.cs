using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementCadenceSchedulerTests
{
    [Fact]
    public void EmptySettlementAdvancesAcrossCanonicalDailyBoundaries()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(303), new SimulationScopeId(30)).CaptureState();
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = [],
        });
        var target = new SimulationTime(
            checked((10 * SettlementSimulation.DayMilliseconds) + (5 * SettlementSimulation.HourMilliseconds)));

        simulation.AdvanceTo(target);

        var captured = simulation.CaptureState();
        var dayEvents = captured.Events
            .Where(entry => string.Equals(entry.Kind, SettlementEventKinds.DayBegan, StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(target, simulation.Time);
        Assert.Equal(10, dayEvents.Length);
        for (var index = 0; index < dayEvents.Length; index++)
        {
            Assert.Equal(
                checked((index + 1L) * SettlementSimulation.DayMilliseconds),
                dayEvents[index].Time.Milliseconds);
        }
    }

    [Fact]
    public void DueDailyBoundaryFailureDoesNotAdvanceTicklessPartition()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(304), new SimulationScopeId(31)).CaptureState();
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = [],
            NextEventId = long.MaxValue,
        });

        Assert.Throws<InvalidOperationException>(() =>
            simulation.AdvanceTo(new SimulationTime(SettlementSimulation.DayMilliseconds)));
        Assert.Equal(new SimulationTime(0), simulation.Time);
        Assert.Empty(simulation.CaptureState().Events);
    }
}
