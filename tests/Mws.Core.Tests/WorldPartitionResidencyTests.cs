using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldPartitionResidencyTests
{
    private static readonly WorldInputKind[] ExpectedResidencyReplayKinds =
    [
        WorldInputKind.EnqueueResidentMigration,
        WorldInputKind.DispatchOutbox,
        WorldInputKind.UnloadSettlement,
        WorldInputKind.DeliverInbox,
        WorldInputKind.LoadSettlement,
        WorldInputKind.DeliverInbox,
    ];

    [Fact]
    public void UnloadedPartitionDefersWorkAndReactivatesToAlwaysLoadedState()
    {
        var alwaysLoaded = WorldRuntime.Create(new WorldSeed(8301));
        var alwaysScope = alwaysLoaded.AddDefaultSettlement();
        var evicted = WorldRuntime.Create(new WorldSeed(8301));
        var evictedScope = evicted.AddDefaultSettlement();

        evicted.UnloadSettlement(evictedScope);
        Assert.False(evicted.IsSettlementLoaded(evictedScope));

        alwaysLoaded.AdvanceHours(2);
        alwaysLoaded.AdvanceHours(3);
        evicted.AdvanceHours(2);
        evicted.AdvanceHours(3);

        var dormantCheckpoint = evicted.CaptureCheckpoint();
        var dormantDescriptor = Assert.Single(dormantCheckpoint.Manifest.Partitions);
        var dormantPartition = Assert.Single(dormantCheckpoint.Partitions);
        Assert.False(dormantDescriptor.IsLoaded);
        Assert.Equal(2L, dormantDescriptor.Revision);
        Assert.Equal(evicted.Time, dormantPartition.Settlement.Time);
        Assert.Equal(
            SettlementStateJson.Serialize(alwaysLoaded.CaptureSettlementState(alwaysScope)),
            SettlementStateJson.Serialize(dormantPartition.Settlement));

        evicted.LoadSettlement(evictedScope);

        Assert.True(evicted.IsSettlementLoaded(evictedScope));
        Assert.Equal(
            SettlementStateJson.Serialize(alwaysLoaded.CaptureSettlementState(alwaysScope)),
            SettlementStateJson.Serialize(evicted.CaptureSettlementState(evictedScope)));
        Assert.Equal(
            Assert.Single(alwaysLoaded.CaptureCheckpoint().Manifest.Partitions).Revision,
            Assert.Single(evicted.CaptureCheckpoint().Manifest.Partitions).Revision);
    }

    [Fact]
    public void TransportBlocksOnUnloadedDestinationAndResidencyReplaysExactly()
    {
        var world = WorldRuntime.Create(new WorldSeed(8302));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var baseline = world.CaptureCheckpoint();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;

        _ = world.EnqueueResidentMigration(residentId, source, destination);
        Assert.Equal(1, world.DispatchOutbox(1));
        world.UnloadSettlement(destination);

        var blocked = world.DeliverInbox(1);
        Assert.Equal(0, blocked.CompletedCount);
        Assert.Equal(WorldTransportCodes.DestinationPartitionUnavailable, blocked.BlockedCode);
        Assert.False(world.IsSettlementLoaded(destination));

        world.LoadSettlement(destination);
        var delivered = world.DeliverInbox(1);
        Assert.Equal(1, delivered.CompletedCount);
        Assert.True(Assert.Single(delivered.Receipts).OperationReceipt.Success);

        var expected = world.CaptureCheckpoint();
        var tail = expected.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();
        Assert.Equal(ExpectedResidencyReplayKinds, tail.Select(entry => entry.Kind).ToArray());

        var replayed = WorldRuntime.ReplayFrom(baseline, tail).CaptureCheckpoint();
        Assert.Equal(CheckpointSignature(expected), CheckpointSignature(replayed));
    }

    [Fact]
    public void UnloadedResidencySurvivesJsonWorldStoreCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mws-residency-store-{Guid.NewGuid():N}");
        try
        {
            var world = WorldRuntime.Create(new WorldSeed(8303));
            var scope = world.AddDefaultSettlement();
            world.UnloadSettlement(scope);
            world.AdvanceHours(4);

            var store = new JsonWorldStore(root);
            store.SaveCheckpoint(world.CreateCheckpoint());

            var manifest = store.LoadManifest();
            Assert.False(Assert.Single(manifest.Partitions).IsLoaded);
            var selectivelyLoadedState = store.LoadSettlement(scope);
            Assert.Equal(world.Time, selectivelyLoadedState.Time);

            var restored = WorldRuntime.Restore(store.LoadCheckpoint());
            Assert.False(restored.IsSettlementLoaded(scope));
            Assert.Equal(
                SettlementStateJson.Serialize(selectivelyLoadedState),
                SettlementStateJson.Serialize(restored.CaptureSettlementState(scope)));

            restored.LoadSettlement(scope);
            Assert.True(restored.IsSettlementLoaded(scope));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void InvalidResidencyTransitionDoesNotConsumeInputSequence()
    {
        var world = WorldRuntime.Create(new WorldSeed(8304));
        var scope = world.AddDefaultSettlement();
        world.UnloadSettlement(scope);
        var before = world.CaptureCheckpoint().Manifest.NextInputSequence;

        Assert.Throws<InvalidOperationException>(() => world.UnloadSettlement(scope));

        Assert.Equal(before, world.CaptureCheckpoint().Manifest.NextInputSequence);
    }

    private static string CheckpointSignature(WorldCheckpointState checkpoint) =>
        string.Join(
            "\n",
            new[] { WorldManifestJson.Serialize(checkpoint.Manifest) }
                .Concat(checkpoint.Partitions
                    .OrderBy(entry => entry.ScopeId.Value)
                    .Select(entry => SettlementStateJson.Serialize(entry.Settlement))));
}
