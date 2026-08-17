using Mws.Domain;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementTimeInvariantTests
{
    [Fact]
    public void RestoreRejectsPersistedTimeOutsideCanonicalWholeHourBoundary()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(801)).CaptureState();
        var invalid = state with
        {
            Time = new SimulationTime(SettlementSimulation.HourMilliseconds / 2),
        };

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(invalid));
    }
}
