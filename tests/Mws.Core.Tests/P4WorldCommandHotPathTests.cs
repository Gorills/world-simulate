using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P4WorldCommandHotPathTests
{
    [Fact]
    public void DirectWorldCommandFailureLeavesSettlementRevisionAndJournalUnchanged()
    {
        var seedWorld = WorldRuntime.Create(new WorldSeed(9401));
        var scope = seedWorld.AddDefaultSettlement();
        var checkpoint = seedWorld.CaptureCheckpoint();
        var poisonedPartitions = checkpoint.Partitions
            .Select(partition => partition.ScopeId == scope
                ? partition with
                {
                    Settlement = partition.Settlement with { NextEventId = long.MaxValue },
                }
                : partition)
            .ToArray();
        var world = WorldRuntime.Restore(checkpoint with { Partitions = poisonedPartitions });
        var state = world.CaptureSettlementState(scope);
        var residentId = state.Residents[0].Id;
        var command = new FeedResidentCommand(new CommandId(state.NextCommandId), residentId);
        var beforeSettlement = SettlementStateJson.Serialize(state);
        var beforeCheckpoint = world.CaptureCheckpoint();
        var beforeRevision = PartitionRevision(beforeCheckpoint, scope);
        var beforeInputSequence = beforeCheckpoint.Manifest.NextInputSequence;

        Assert.Throws<InvalidOperationException>(() =>
            world.ExecuteSettlementCommand(scope, command));

        var afterCheckpoint = world.CaptureCheckpoint();
        Assert.Equal(beforeSettlement, SettlementStateJson.Serialize(world.CaptureSettlementState(scope)));
        Assert.Equal(beforeRevision, PartitionRevision(afterCheckpoint, scope));
        Assert.Equal(beforeInputSequence, afterCheckpoint.Manifest.NextInputSequence);
    }

    [Fact]
    public void RevisionExhaustionRejectsFreshCommandButAllowsDuplicateWithoutDoubleApply()
    {
        var seedWorld = WorldRuntime.Create(new WorldSeed(9402));
        var scope = seedWorld.AddDefaultSettlement();
        var checkpoint = seedWorld.CaptureCheckpoint();
        var nearMaxRevision = long.MaxValue - 1;
        var nearExhaustedManifest = checkpoint.Manifest with
        {
            Partitions = checkpoint.Manifest.Partitions
                .Select(partition => partition.ScopeId == scope
                    ? partition with { Revision = nearMaxRevision }
                    : partition)
                .ToArray(),
        };
        var nearExhaustedPartitions = checkpoint.Partitions
            .Select(partition => partition.ScopeId == scope
                ? partition with { Revision = nearMaxRevision }
                : partition)
            .ToArray();
        var world = WorldRuntime.Restore(checkpoint with
        {
            Manifest = nearExhaustedManifest,
            Partitions = nearExhaustedPartitions,
        });
        var initialState = world.CaptureSettlementState(scope);
        var residentId = initialState.Residents[0].Id;
        var command = new FeedResidentCommand(new CommandId(initialState.NextCommandId), residentId);

        var first = world.ExecuteSettlementCommand(scope, command);
        var afterFirstSettlement = SettlementStateJson.Serialize(world.CaptureSettlementState(scope));
        var afterFirstCheckpoint = world.CaptureCheckpoint();

        Assert.True(first.Success);
        Assert.Equal(long.MaxValue, PartitionRevision(afterFirstCheckpoint, scope));

        var duplicate = world.ExecuteSettlementCommand(scope, command);
        var afterDuplicateCheckpoint = world.CaptureCheckpoint();

        Assert.Equal(first.Success, duplicate.Success);
        Assert.Equal(first.Code, duplicate.Code);
        Assert.Equal(first.SubjectId, duplicate.SubjectId);
        Assert.Equal(first.Facts.ToArray(), duplicate.Facts.ToArray());
        Assert.Equal(afterFirstSettlement, SettlementStateJson.Serialize(world.CaptureSettlementState(scope)));
        Assert.Equal(long.MaxValue, PartitionRevision(afterDuplicateCheckpoint, scope));

        var freshState = world.CaptureSettlementState(scope);
        var fresh = new FeedResidentCommand(new CommandId(freshState.NextCommandId), residentId);
        var beforeFreshSettlement = SettlementStateJson.Serialize(freshState);
        var beforeFreshCheckpoint = world.CaptureCheckpoint();
        var beforeFreshInputSequence = beforeFreshCheckpoint.Manifest.NextInputSequence;

        Assert.Throws<InvalidOperationException>(() =>
            world.ExecuteSettlementCommand(scope, fresh));

        var afterFreshCheckpoint = world.CaptureCheckpoint();
        Assert.Equal(beforeFreshSettlement, SettlementStateJson.Serialize(world.CaptureSettlementState(scope)));
        Assert.Equal(long.MaxValue, PartitionRevision(afterFreshCheckpoint, scope));
        Assert.Equal(beforeFreshInputSequence, afterFreshCheckpoint.Manifest.NextInputSequence);
    }

    private static long PartitionRevision(WorldCheckpointState checkpoint, SimulationScopeId scopeId) =>
        checkpoint.Manifest.Partitions.Single(partition => partition.ScopeId == scopeId).Revision;
}
