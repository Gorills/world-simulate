using Mws.Domain;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementTimeInvariantTests
{
    [Fact]
    public void RestoreAcceptsNonNegativeSubHourTimeAndRejectsNegativeTime()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(801)).CaptureState();
        var subHour = state with
        {
            Time = new SimulationTime(SettlementSimulation.HourMilliseconds / 2),
        };

        var restored = SettlementSimulation.Restore(subHour);

        Assert.Equal(subHour.Time, restored.Time);

        var negative = state with
        {
            Time = new SimulationTime(-1),
        };

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(negative));
    }
}
