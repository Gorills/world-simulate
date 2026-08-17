using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3SemanticLocationFoundationTests
{
    [Fact]
    public void ResidentAndPlayerSemanticLocationsPersistAndProject()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9301));
        var projection = simulation.Project();

        Assert.All(projection.Residents, resident =>
        {
            var location = Assert.IsType<SettlementActorLocationProjection>(resident.Location);
            Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
            Assert.Equal(SettlementPlaceKind.Home, location.CurrentPlace.Kind);
            Assert.Equal(resident.HomeId, location.CurrentPlace.EntityId);
            Assert.Equal(location.CurrentPlace, location.DestinationPlace);
            Assert.Null(location.Travel);
        });

        var json = SettlementStateJson.Serialize(simulation.CaptureState());
        var restoredSettlement = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        Assert.Equal(json, SettlementStateJson.Serialize(restoredSettlement.CaptureState()));

        var world = WorldRuntime.Create(new WorldSeed(9302));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var player = world.ProjectPlayer();
        var playerLocation = Assert.IsType<SettlementActorLocationProjection>(player.Location);

        Assert.Equal(SettlementActorLocationKind.AtPlace, playerLocation.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, playerLocation.CurrentPlace);
        Assert.Equal(playerLocation.CurrentPlace, playerLocation.DestinationPlace);
        Assert.Null(playerLocation.Travel);

        var restoredWorld = WorldRuntime.Restore(world.CaptureCheckpoint());
        Assert.Equal(player.Location, restoredWorld.ProjectPlayer().Location);
    }

    [Fact]
    public void ExplicitSettlementLocationDoesNotBecomeClockDerivedAfterSaveLoad()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9306)).CaptureState();
        var resident = state.Residents[0];
        var explicitSettlement = SettlementActorLocationState.At(SettlementPlaceRef.Settlement);
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id ? entry with { Location = explicitSettlement } : entry)
            .ToArray();
        var simulation = SettlementSimulation.Restore(state with
        {
            Time = new SimulationTime(8 * SettlementSimulation.HourMilliseconds),
            Residents = residents,
        });

        var captured = simulation.CaptureState();
        var capturedResident = Assert.Single(captured.Residents, entry => entry.Id == resident.Id);
        Assert.Equal(explicitSettlement, capturedResident.Location);

        var json = SettlementStateJson.Serialize(captured);
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        var location = RequireLocation(restored, resident.Id);

        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, location.CurrentPlace);
        Assert.Equal(location.CurrentPlace, location.DestinationPlace);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void CurrentCompactResidenceEncodingIsIndependentOfClock()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9307)).CaptureState();

        Assert.Equal(
            SettlementVersions.CurrentResidentLocationEncodingVersion,
            state.ResidentLocationEncodingVersion);
        Assert.All(state.Residents, resident => Assert.Null(resident.Location));

        var restored = SettlementSimulation.Restore(state with
        {
            Time = new SimulationTime(8 * SettlementSimulation.HourMilliseconds),
        });

        Assert.All(restored.Project().Residents, resident =>
        {
            var location = Assert.IsType<SettlementActorLocationProjection>(resident.Location);
            Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
            Assert.Equal(SettlementPlaceKind.Home, location.CurrentPlace.Kind);
            Assert.Equal(resident.HomeId, location.CurrentPlace.EntityId);
        });
    }

    [Fact]
    public void LegacyCompactResidenceEncodingKeepsOldClockHydrationIsolated()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9308)).CaptureState();
        var restored = SettlementSimulation.Restore(state with
        {
            Time = new SimulationTime(8 * SettlementSimulation.HourMilliseconds),
            ResidentLocationEncodingVersion = SettlementVersions.LegacyResidentLocationEncodingVersion,
        });

        Assert.All(restored.Project().Residents, resident =>
        {
            var location = Assert.IsType<SettlementActorLocationProjection>(resident.Location);
            Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
            Assert.Equal(SettlementPlaceKind.Workplace, location.CurrentPlace.Kind);
            Assert.Equal(resident.WorkplaceId, location.CurrentPlace.EntityId);
        });
    }

    [Fact]
    public void ActiveTravelProgressPersistsAcrossHoursAndSaveLoad()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9304)).CaptureState();
        var resident = state.Residents[0];
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var travel = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            home,
            SettlementPlaceRef.Settlement,
            new SettlementTravelProgressState(
                2 * SettlementSimulation.HourMilliseconds,
                ElapsedMilliseconds: 0));
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id ? entry with { Location = travel } : entry)
            .ToArray();
        var simulation = SettlementSimulation.Restore(state with { Residents = residents });

        simulation.AdvanceHours(1);
        var afterOneHour = RequireLocation(simulation, resident.Id);

        Assert.Equal(SettlementActorLocationKind.Travelling, afterOneHour.Kind);
        Assert.Equal(home, afterOneHour.CurrentPlace);
        Assert.Equal(SettlementPlaceRef.Settlement, afterOneHour.DestinationPlace);
        Assert.NotNull(afterOneHour.Travel);
        Assert.Equal(2 * SettlementSimulation.HourMilliseconds, afterOneHour.Travel.DurationMilliseconds);
        Assert.Equal(SettlementSimulation.HourMilliseconds, afterOneHour.Travel.ElapsedMilliseconds);

        var json = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        Assert.Equal(afterOneHour, RequireLocation(restored, resident.Id));

        restored.AdvanceHours(1);
        var arrived = RequireLocation(restored, resident.Id);

        Assert.Equal(SettlementActorLocationKind.AtPlace, arrived.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, arrived.CurrentPlace);
        Assert.Equal(arrived.CurrentPlace, arrived.DestinationPlace);
        Assert.Null(arrived.Travel);
    }

    [Fact]
    public void RestoreRejectsContradictoryAtPlaceLocation()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9303)).CaptureState();
        var resident = state.Residents[0];
        var invalidLocation = new SettlementActorLocationState(
            SettlementActorLocationKind.AtPlace,
            SettlementPlaceRef.Settlement,
            new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId));
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id ? entry with { Location = invalidLocation } : entry)
            .ToArray();

        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(state with { Residents = residents }));
    }

    [Fact]
    public void RestoreRejectsInvalidActiveTravelProgress()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9305)).CaptureState();
        var resident = state.Residents[0];
        var invalidLocation = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            SettlementPlaceRef.Settlement,
            new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId),
            new SettlementTravelProgressState(DurationMilliseconds: 0, ElapsedMilliseconds: 0));
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id ? entry with { Location = invalidLocation } : entry)
            .ToArray();

        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(state with { Residents = residents }));
    }

    private static SettlementActorLocationProjection RequireLocation(
        SettlementSimulation simulation,
        EntityId residentId)
    {
        var resident = simulation.Project().Residents.Single(entry => entry.Id == residentId);
        return Assert.IsType<SettlementActorLocationProjection>(resident.Location);
    }
}
