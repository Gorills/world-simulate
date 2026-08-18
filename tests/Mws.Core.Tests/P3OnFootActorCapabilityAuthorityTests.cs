using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3OnFootActorCapabilityAuthorityTests
{
    private const string CapabilityProvenance = "fixture:test-on-foot-actor-capability";

    [Fact]
    public void DefaultResidentAndPlayerCapabilityRemainUnknown()
    {
        var settlement = SettlementSimulation.CreateDefault(new WorldSeed(9360)).CaptureState();
        Assert.All(settlement.Residents, AssertUnknownCapability);

        var world = WorldRuntime.Create(new WorldSeed(9361));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var player = world.CaptureCheckpoint().Manifest.Player
            ?? throw new InvalidOperationException("Test fixture is missing player actor.");

        Assert.Equal(SettlementOnFootActorCapabilityClass.Unknown, player.OnFootCapability);
        Assert.Null(player.OnFootCapabilityProvenanceReference);
        Assert.False(player.IsOnFootCapabilityFixture);
    }

    [Fact]
    public void ResidentCapabilitySurvivesSettlementStateRoundTripWithoutStartingTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9362)).CaptureState();
        var resident = state.Residents[0] with
        {
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
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
            SettlementOnFootActorCapabilityClass.BaselineCompatible,
            restoredResident.OnFootCapability);
        Assert.Equal(CapabilityProvenance, restoredResident.OnFootCapabilityProvenanceReference);
        Assert.True(restoredResident.IsOnFootCapabilityFixture);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void PlayerCapabilitySurvivesCheckpointRestoreWithoutChangingLocation()
    {
        var world = WorldRuntime.Create(new WorldSeed(9363));
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
                    OnFootCapability = SettlementOnFootActorCapabilityClass.NonBaseline,
                    OnFootCapabilityProvenanceReference = CapabilityProvenance,
                    IsOnFootCapabilityFixture = true,
                },
            },
        };

        var restored = WorldRuntime.Restore(prepared);
        var capturedPlayer = restored.CaptureCheckpoint().Manifest.Player
            ?? throw new InvalidOperationException("Restored checkpoint is missing player actor.");
        var location = Assert.IsType<SettlementActorLocationProjection>(
            restored.ProjectPlayer().Location);

        Assert.Equal(
            SettlementOnFootActorCapabilityClass.NonBaseline,
            capturedPlayer.OnFootCapability);
        Assert.Equal(CapabilityProvenance, capturedPlayer.OnFootCapabilityProvenanceReference);
        Assert.True(capturedPlayer.IsOnFootCapabilityFixture);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void ResidentCapabilityMovesWithResidentAcrossSettlementMigration()
    {
        var world = WorldRuntime.Create(new WorldSeed(9364));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var sourcePartition = Assert.Single(
            checkpoint.Partitions,
            entry => entry.ScopeId == source);
        var resident = sourcePartition.Settlement.Residents[0] with
        {
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
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
            SettlementOnFootActorCapabilityClass.BaselineCompatible,
            migrated.OnFootCapability);
        Assert.Equal(CapabilityProvenance, migrated.OnFootCapabilityProvenanceReference);
        Assert.True(migrated.IsOnFootCapabilityFixture);
    }

    [Fact]
    public void ResidentAndPlayerRejectUnsupportedOrUnprovenCapabilityAuthority()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9365)).CaptureState();
        var resident = state.Residents[0];

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            }),
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCapability = (SettlementOnFootActorCapabilityClass)999,
            }),
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            Residents = ReplaceResident(state, resident with
            {
                OnFootCapabilityProvenanceReference = CapabilityProvenance,
            }),
        }));

        var world = WorldRuntime.Create(new WorldSeed(9366));
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
                    OnFootCapability = SettlementOnFootActorCapabilityClass.NonBaseline,
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

    private static void AssertUnknownCapability(ResidentState resident)
    {
        Assert.Equal(SettlementOnFootActorCapabilityClass.Unknown, resident.OnFootCapability);
        Assert.Null(resident.OnFootCapabilityProvenanceReference);
        Assert.False(resident.IsOnFootCapabilityFixture);
    }
}
