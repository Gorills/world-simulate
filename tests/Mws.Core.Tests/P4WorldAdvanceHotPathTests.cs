using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P4WorldAdvanceHotPathTests
{
    [Fact]
    public void FailedLaterPartitionAdvanceRestoresBoundedEventHistoryAndWorldCommitState()
    {
        var seedWorld = WorldRuntime.Create(new WorldSeed(9403));
        var first = seedWorld.AddDefaultSettlement();
        var second = seedWorld.AddDefaultSettlement();
        seedWorld.AdvanceHours(23);
        var checkpoint = seedWorld.CaptureCheckpoint();
        var retainedEvents = Enumerable.Range(1, SettlementSimulation.MaxRetainedEvents)
            .Select(id => new SettlementEvent(
                id,
                new SimulationTime(0),
                SettlementEventKinds.DayBegan,
                null,
                Array.Empty<SettlementFact>()))
            .ToArray();
        var partitions = checkpoint.Partitions
            .Select(partition => partition.ScopeId == first
                ? partition with
                {
                    Settlement = partition.Settlement with
                    {
                        NextEventId = SettlementSimulation.MaxRetainedEvents + 1L,
                        Events = retainedEvents,
                    },
                }
                : partition.ScopeId == second
                    ? partition with
                    {
                        Settlement = partition.Settlement with { NextEventId = long.MaxValue },
                    }
                    : partition)
            .ToArray();
        var world = WorldRuntime.Restore(checkpoint with { Partitions = partitions });
        var beforeFirst = SettlementStateJson.Serialize(world.CaptureSettlementState(first));
        var beforeCheckpoint = world.CaptureCheckpoint();
        var beforeFirstRevision = PartitionRevision(beforeCheckpoint, first);
        var beforeSecondRevision = PartitionRevision(beforeCheckpoint, second);
        var beforeInputSequence = beforeCheckpoint.Manifest.NextInputSequence;

        Assert.Throws<InvalidOperationException>(() => world.AdvanceHours(1));

        var afterCheckpoint = world.CaptureCheckpoint();
        var afterFirst = world.CaptureSettlementState(first);
        Assert.Equal(beforeFirst, SettlementStateJson.Serialize(afterFirst));
        Assert.Equal(1L, afterFirst.Events[0].Id);
        Assert.Equal(SettlementSimulation.MaxRetainedEvents, afterFirst.Events.Count);
        Assert.Equal(23 * SettlementSimulation.HourMilliseconds, world.Time.Milliseconds);
        Assert.Equal(beforeFirstRevision, PartitionRevision(afterCheckpoint, first));
        Assert.Equal(beforeSecondRevision, PartitionRevision(afterCheckpoint, second));
        Assert.Equal(beforeInputSequence, afterCheckpoint.Manifest.NextInputSequence);
    }

    private static long PartitionRevision(WorldCheckpointState checkpoint, SimulationScopeId scopeId) =>
        checkpoint.Manifest.Partitions.Single(partition => partition.ScopeId == scopeId).Revision;
}
