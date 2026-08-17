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

        var restoredWorld = WorldRuntime.Restore(world.CaptureCheckpoint());
        Assert.Equal(player.Location, restoredWorld.ProjectPlayer().Location);
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
}
