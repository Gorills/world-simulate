using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class DeterminismTests
{
    [Fact]
    public void Same_seed_produces_same_snapshots()
    {
        var left = new DeterministicWorldSimulation(new WorldSeed(42));
        var right = new DeterministicWorldSimulation(new WorldSeed(42));

        for (var i = 0; i < 1_000; i++)
        {
            Assert.Equal(left.Step(), right.Step());
        }
    }

    [Fact]
    public void Snapshot_round_trip_preserves_state()
    {
        var simulation = new DeterministicWorldSimulation(new WorldSeed(7));
        var snapshot = simulation.Step();

        var json = WorldSnapshotJson.Serialize(snapshot);
        var restored = WorldSnapshotJson.Deserialize(json);

        Assert.Equal(snapshot, restored);
    }
}
