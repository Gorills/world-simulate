using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldPlayerActorTests
{
    [Fact]
    public void PlayerActorIdentityInventoryAndScopeSurvivePersistenceAndReplay()
    {
        var world = WorldRuntime.Create(new WorldSeed(9201));
        var baseline = world.CaptureCheckpoint();
        var scope = world.AddDefaultSettlement();
        var playerId = world.AddPlayerActor(scope);
        world.AdvanceHours(8);

        var player = world.ProjectPlayer();
        var ration = Assert.Single(player.Inventory, item => item.ItemId == SettlementItems.Ration);
        Assert.Equal(playerId, player.Id);
        Assert.Equal(scope, player.ScopeId);
        Assert.Equal(2, ration.Quantity);
        Assert.True(world.TryGetEntityLocation(playerId, out var playerScope));
        Assert.Equal(scope, playerScope);

        var checkpoint = world.CaptureCheckpoint();
        Assert.Equal(playerId, checkpoint.Manifest.Player?.Id);
        Assert.Equal(
            WorldPlayerLocationVersions.CurrentEncodingVersion,
            checkpoint.Manifest.Player?.LocationEncodingVersion);
        Assert.Equal(
            new[]
            {
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.AddPlayerActor,
                WorldInputKind.AdvanceTo,
            },
            checkpoint.Manifest.InputJournal.Select(entry => entry.Kind).ToArray());

        var manifestJson = WorldManifestJson.Serialize(checkpoint.Manifest);
        var restoredManifest = WorldManifestJson.Deserialize(manifestJson);
        var restored = WorldRuntime.Restore(checkpoint with { Manifest = restoredManifest });
        AssertPlayerEquivalent(player, restored.ProjectPlayer());

        var tail = checkpoint.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();
        var replayed = WorldRuntime.ReplayFrom(baseline, tail);
        AssertPlayerEquivalent(player, replayed.ProjectPlayer());
        Assert.Equal(
            WorldManifestJson.Serialize(checkpoint.Manifest),
            WorldManifestJson.Serialize(replayed.CaptureCheckpoint().Manifest));
    }

    [Fact]
    public void PlayerActorRejectsDuplicateCreationAndInvalidInventoryRestore()
    {
        var world = WorldRuntime.Create(new WorldSeed(9202));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);

        Assert.Throws<InvalidOperationException>(() => world.AddPlayerActor(scope));

        var checkpoint = world.CaptureCheckpoint();
        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");
        var corrupted = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Player = player with
                {
                    Inventory =
                    [
                        new WorldPlayerInventoryItemState(string.Empty, 1),
                    ],
                },
            },
        };

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.Restore(corrupted));
    }

    [Fact]
    public void CurrentPlayerLocationEncodingRejectsMissingTravelProgress()
    {
        var world = WorldRuntime.Create(new WorldSeed(9203));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");
        var resident = checkpoint.Partitions[0].Settlement.Residents[0];
        var missingProgress = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            SettlementPlaceRef.Settlement,
            new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId));
        var corrupted = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Player = player with
                {
                    Location = missingProgress,
                    LocationEncodingVersion = WorldPlayerLocationVersions.CurrentEncodingVersion,
                },
            },
        };

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.Restore(corrupted));
    }

    [Fact]
    public void LegacyPlayerLocationEncodingHydratesMissingTravelProgressOnlyForCompatibility()
    {
        var world = WorldRuntime.Create(new WorldSeed(9204));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");
        var resident = checkpoint.Partitions[0].Settlement.Residents[0];
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var missingProgress = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            SettlementPlaceRef.Settlement,
            workplace);
        var legacy = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Player = player with
                {
                    Location = missingProgress,
                    LocationEncodingVersion = WorldPlayerLocationVersions.LegacyEncodingVersion,
                },
            },
        };

        var restored = WorldRuntime.Restore(legacy);
        var location = Assert.IsType<SettlementActorLocationProjection>(restored.ProjectPlayer().Location);

        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, location.CurrentPlace);
        Assert.Equal(workplace, location.DestinationPlace);
        Assert.NotNull(location.Travel);
        Assert.Equal(SettlementSimulation.HourMilliseconds, location.Travel.DurationMilliseconds);
        Assert.Equal(0, location.Travel.ElapsedMilliseconds);
        Assert.Equal(
            WorldPlayerLocationVersions.CurrentEncodingVersion,
            restored.CaptureCheckpoint().Manifest.Player?.LocationEncodingVersion);
    }

    private static void AssertPlayerEquivalent(WorldPlayerProjection expected, WorldPlayerProjection actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ScopeId, actual.ScopeId);
        Assert.Equal(expected.Inventory.ToArray(), actual.Inventory.ToArray());
        Assert.Equal(expected.Location, actual.Location);
    }
}
