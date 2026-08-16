using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class DeterminismTests
{
    [Fact]
    public void SameSeedProducesSameSnapshots()
    {
        var left = new DeterministicWorldSimulation(new WorldSeed(42));
        var right = new DeterministicWorldSimulation(new WorldSeed(42));

        for (var i = 0; i < 1_000; i++)
        {
            Assert.Equal(left.Advance(), right.Advance());
        }
    }

    [Fact]
    public void SnapshotRoundTripPreservesState()
    {
        var simulation = new DeterministicWorldSimulation(new WorldSeed(7));
        var snapshot = simulation.Advance();

        var json = WorldSnapshotJson.Serialize(snapshot);
        var restored = WorldSnapshotJson.Deserialize(json);

        Assert.Equal(snapshot, restored);
    }
}
