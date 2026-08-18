using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3OnFootCarriedLoadAuthorityTests
{
    private const string LoadProvenance = "fixture:test-on-foot-carried-load";

    [Fact]
    public void DefaultResidentAndPlayerCarriedLoadRemainUnknownDespiteInventory()
    {
        var settlement = SettlementSimulation.CreateDefault(new WorldSeed(9370)).CaptureState();
        Assert.All(settlement.Residents, AssertUnknownCarriedLoad);

        var world = WorldRuntime.Create(new WorldSeed(9371));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var player = world.CaptureCheckpoint().Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");

        Assert.NotEmpty(player.Inventory);
        Assert.Equal(SettlementOnFootCarriedLoadClass.Unknown, player.OnFootCarriedLoad);
        Assert.Null(player.OnFootCarriedLoadProvenanceReference);
        Assert.False(player.IsOnFootCarriedLoadFixture);
    }

    [Fact]
    public void ResidentCarriedLoadSurvivesSettlementStateRoundTripWithoutStartingTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9372)).CaptureState();
        var resident = state.Residents[0] with
        {
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident),
        });

        var captured = simulation.CaptureState();
        var decoded = SettlementStateJson.Deserialize(SettlementStateJson.Serialize(captured));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredResident = Assert.Single(
            restored.CaptureState().Residents,
            entry => entry.Id == resident.Id);
        var projected = Assert.Single(
            restored.Project().Residents,
            entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(
            SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            restoredResident.OnFootCarriedLoad);
        Assert.Equal(LoadProvenance, restoredResident.OnFootCarriedLoadProvenanceReference);
        Assert.True(restoredResident.IsOnFootCarriedLoadFixture);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void PlayerCarriedLoadSurvivesCheckpointRestoreWithoutChangingLocation()
    {
        var world = WorldRuntime.Create(new WorldSeed(9373));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");
        var prepared = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Player = player with
                {
                    OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
                    OnFootCarriedLoadProvenanceReference = LoadProvenance,
                    IsOnFootCarriedLoadFixture = true,
                },
            },
        };

        var restored = WorldRuntime.Restore(prepared);
        var capturedPlayer = restored.CaptureCheckpoint().Manifest.Player
            ?? throw new InvalidOperationException("Restored checkpoint is missing player actor.");
        var location = Assert.IsType<SettlementActorLocationProjection>(
            restored.ProjectPlayer().Location);

        Assert.Equal(
            SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
            capturedPlayer.OnFootCarriedLoad);
        Assert.Equal(LoadProvenance, capturedPlayer.OnFootCarriedLoadProvenanceReference);
        Assert.True(capturedPlayer.IsOnFootCarriedLoadFixture);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void ResidentCarriedLoadMovesWithResidentAcrossSettlementMigration()
    {
        var world = WorldRuntime.Create(new WorldSeed(9374));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var sourcePartition = Assert.Single(
            checkpoint.Partitions,
            entry => entry.ScopeId == source);
        var resident = sourcePartition.Settlement.Residents[0] with
        {
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var nextSource = sourcePartition with
        {
            Settlement = sourcePartition.Settlement with
            {
                Residents = ReplaceResident(sourcePartition.Settlement, resident),
            },
        };
        var restored = WorldRuntime.Restore(checkpoint with
        {
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == source ? nextSource : entry)
                .ToArray(),
        });

        var result = restored.MigrateResident(
            restored.AllocateOperationId(),
            resident.Id,
            source,
            destination);
        var migrated = Assert.Single(
            restored.CaptureSettlementState(destination).Residents,
            entry => entry.Id == resident.Id);

        Assert.True(result.Success);
        Assert.Equal("MIGRATED", result.Code);
        Assert.Equal(
            SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
            migrated.OnFootCarriedLoad);
        Assert.Equal(LoadProvenance, migrated.OnFootCarriedLoadProvenanceReference);
        Assert.True(migrated.IsOnFootCarriedLoadFixture);
    }

    [Fact]
    public void ResidentAndPlayerRejectUnsupportedOrUnprovenCarriedLoadAuthority()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9375)).CaptureState();
        var resident = state.Residents[0];

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            }),
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCarriedLoad = (SettlementOnFootCarriedLoadClass)999,
            }),
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCarriedLoadProvenanceReference = LoadProvenance,
            }),
        }));

        var world = WorldRuntime.Create(new WorldSeed(9376));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.Restore(checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Player = player with
                {
                    OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
                },
            },
        }));
    }

    private static ResidentState[] ReplaceResident(
        SettlementState state,
        ResidentState replacement) =>
        state.Residents
            .Select(entry => entry.Id == replacement.Id ? replacement : entry)
            .ToArray();

    private static void AssertUnknownCarriedLoad(ResidentState resident)
    {
        Assert.Equal(SettlementOnFootCarriedLoadClass.Unknown, resident.OnFootCarriedLoad);
        Assert.Null(resident.OnFootCarriedLoadProvenanceReference);
        Assert.False(resident.IsOnFootCarriedLoadFixture);
    }
}
