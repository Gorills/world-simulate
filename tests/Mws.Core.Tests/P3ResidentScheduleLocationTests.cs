using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3ResidentScheduleLocationTests
{
    [Fact]
    public void ResidentCommutesBetweenHomeAndWorkplaceOnAuthoritativeHours()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9310));
        var initial = Mira(simulation);
        var initialLocation = RequireLocation(initial);

        Assert.Equal(SettlementActorLocationKind.AtPlace, initialLocation.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Home, initial.HomeId), initialLocation.CurrentPlace);

        simulation.AdvanceHours(7);
        var commutingToWork = Mira(simulation);
        var outbound = RequireLocation(commutingToWork);

        Assert.Equal(SettlementActorLocationKind.Travelling, outbound.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Home, initial.HomeId), outbound.CurrentPlace);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Workplace, initial.WorkplaceId), outbound.DestinationPlace);
        Assert.NotEqual(ResidentActivity.Working, commutingToWork.Activity);

        var travellingState = simulation.CaptureState();
        Assert.NotNull(travellingState.Residents.Single(resident => resident.Id == initial.Id).Location);
        var travellingJson = SettlementStateJson.Serialize(travellingState);
        var restoredTravelling = SettlementSimulation.Restore(SettlementStateJson.Deserialize(travellingJson));
        Assert.Equal(outbound, RequireLocation(Mira(restoredTravelling)));

        simulation.AdvanceHours(1);
        var atWork = Mira(simulation);
        var workLocation = RequireLocation(atWork);

        Assert.Equal(SettlementActorLocationKind.AtPlace, workLocation.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Workplace, initial.WorkplaceId), workLocation.CurrentPlace);
        Assert.Equal(ResidentActivity.Working, atWork.Activity);
        Assert.Null(simulation.CaptureState().Residents.Single(resident => resident.Id == initial.Id).Location);

        simulation.AdvanceHours(9);
        var commutingHome = Mira(simulation);
        var inbound = RequireLocation(commutingHome);

        Assert.Equal(SettlementActorLocationKind.Travelling, inbound.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Workplace, initial.WorkplaceId), inbound.CurrentPlace);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Home, initial.HomeId), inbound.DestinationPlace);
        Assert.NotEqual(ResidentActivity.Working, commutingHome.Activity);

        simulation.AdvanceHours(1);
        var backHome = Mira(simulation);
        var homeLocation = RequireLocation(backHome);

        Assert.Equal(SettlementActorLocationKind.AtPlace, homeLocation.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Home, initial.HomeId), homeLocation.CurrentPlace);
    }

    [Fact]
    public void StableScheduledLocationRoundTripsThroughCompactPersistence()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9311));
        simulation.AdvanceHours(8);

        var state = simulation.CaptureState();
        Assert.All(state.Residents, resident => Assert.Null(resident.Location));

        var json = SettlementStateJson.Serialize(state);
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));

        Assert.Equal(json, SettlementStateJson.Serialize(restored.CaptureState()));
        Assert.All(restored.Project().Residents, resident =>
        {
            var location = RequireLocation(resident);
            Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
            Assert.Equal(SettlementPlaceKind.Workplace, location.CurrentPlace.Kind);
            Assert.Equal(resident.WorkplaceId, location.CurrentPlace.EntityId);
        });
    }

    private static ResidentProjection Mira(SettlementSimulation simulation) =>
        simulation.Project().Residents.Single(resident => resident.Name == "Mira");

    private static SettlementActorLocationProjection RequireLocation(ResidentProjection resident) =>
        Assert.IsType<SettlementActorLocationProjection>(resident.Location);
}
