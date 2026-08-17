using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldReplayJournalTests
{
    [Fact]
    public void ReplayTailReconstructsAuthoritativeWorldState()
    {
        var world = WorldRuntime.Create(new WorldSeed(7001));
        var baseline = world.CaptureCheckpoint();

        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        world.AdvanceHours(5);

        var residentId = world.CaptureSettlementState(source).Residents[0].Id;
        var command = new FeedResidentCommand(new CommandId(1), residentId);
        var firstCommand = world.ExecuteSettlementCommand(source, command);
        var duplicateCommand = world.ExecuteSettlementCommand(source, command);
        var operationId = world.AllocateOperationId();
        var migration = world.MigrateResident(operationId, residentId, source, destination);
        world.AdvanceHours(19);

        Assert.True(firstCommand.Success);
        Assert.Equal(firstCommand.Success, duplicateCommand.Success);
        Assert.Equal(firstCommand.Code, duplicateCommand.Code);
        Assert.Equal(firstCommand.SubjectId, duplicateCommand.SubjectId);
        Assert.Equal(firstCommand.Facts.ToArray(), duplicateCommand.Facts.ToArray());
        Assert.True(migration.Success);

        var expected = world.CaptureCheckpoint();
        var tail = expected.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();

        var replayed = WorldRuntime.ReplayFrom(baseline, tail).CaptureCheckpoint();

        Assert.Equal(CheckpointSignature(expected), CheckpointSignature(replayed));
        Assert.Equal(
            new[]
            {
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.AdvanceTo,
                WorldInputKind.SettlementCommand,
                WorldInputKind.SettlementCommand,
                WorldInputKind.AllocateOperationId,
                WorldInputKind.ResidentMigration,
                WorldInputKind.AdvanceTo,
            },
            tail.Select(entry => entry.Kind).ToArray());
        Assert.Equal(4, expected.Partitions.Single(entry => entry.ScopeId == source).Revision);
        Assert.Equal(3, expected.Partitions.Single(entry => entry.ScopeId == destination).Revision);
    }

    [Fact]
    public void ReplayRejectsSequenceGapOrWrongCheckpointTime()
    {
        var world = WorldRuntime.Create(new WorldSeed(7002));
        var baseline = world.CaptureCheckpoint();
        _ = world.AddDefaultSettlement();
        var entry = Assert.Single(world.CaptureCheckpoint().Manifest.InputJournal);

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.ReplayFrom(
            baseline,
            [entry with { Sequence = entry.Sequence + 1 }]));
        Assert.Throws<InvalidOperationException>(() => WorldRuntime.ReplayFrom(
            baseline,
            [entry with
            {
                RecordedAt = entry.RecordedAt.AddMilliseconds(SettlementSimulation.HourMilliseconds),
            }]));
    }

    [Fact]
    public void WorldSettlementCommandRevisionChangesOnlyForFreshCommand()
    {
        var world = WorldRuntime.Create(new WorldSeed(7003));
        var scope = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(scope).Residents[0].Id;
        var command = new FeedResidentCommand(new CommandId(1), residentId);

        _ = world.ExecuteSettlementCommand(scope, command);
        _ = world.ExecuteSettlementCommand(scope, command);

        var checkpoint = world.CaptureCheckpoint();
        Assert.Equal(1, checkpoint.Partitions.Single().Revision);
        Assert.Equal(3, checkpoint.Manifest.InputJournal.Count);
        Assert.Equal(
            new[]
            {
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.SettlementCommand,
                WorldInputKind.SettlementCommand,
            },
            checkpoint.Manifest.InputJournal.Select(entry => entry.Kind).ToArray());
    }

    [Fact]
    public void InputJournalRetentionIsBoundedAndManifestRoundTrips()
    {
        var world = WorldRuntime.Create(new WorldSeed(7004));
        for (var index = 0; index < 4_105; index++)
        {
            _ = world.AllocateOperationId();
        }

        var manifest = world.CaptureCheckpoint().Manifest;
        var roundTrip = WorldManifestJson.Deserialize(WorldManifestJson.Serialize(manifest));

        Assert.Equal(4_096, manifest.InputJournal.Count);
        Assert.True(manifest.InputJournalFloor > 1);
        Assert.Equal(manifest.InputJournalFloor, manifest.InputJournal[0].Sequence);
        Assert.Equal(manifest.NextInputSequence - 1, manifest.InputJournal[^1].Sequence);
        Assert.Equal(WorldManifestJson.Serialize(manifest), WorldManifestJson.Serialize(roundTrip));
    }

    [Fact]
    public void RestoreRejectsJournalWithMissingRetainedEntry()
    {
        var world = WorldRuntime.Create(new WorldSeed(7005));
        _ = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var corrupted = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                InputJournal = Array.Empty<WorldInputJournalEntry>(),
            },
        };

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.Restore(corrupted));
    }

    private static string CheckpointSignature(WorldCheckpointState checkpoint) =>
        string.Join(
            "\n",
            new[] { WorldManifestJson.Serialize(checkpoint.Manifest) }
                .Concat(checkpoint.Partitions
                    .OrderBy(entry => entry.ScopeId.Value)
                    .Select(entry => SettlementStateJson.Serialize(entry.Settlement))));
}
