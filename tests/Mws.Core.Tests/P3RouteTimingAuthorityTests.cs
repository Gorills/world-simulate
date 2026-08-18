using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3RouteTimingAuthorityTests
{
    [Theory]
    [InlineData(SettlementOnFootRouteTimingClass.Unknown)]
    [InlineData(SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed)]
    [InlineData(SettlementOnFootRouteTimingClass.NonBaseline)]
    public void TimingClassDoesNotReplaceRouteAuthority(
        SettlementOnFootRouteTimingClass timingClass)
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9350)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident) with { OnFootTimingClass = timingClass };
        var simulation = SettlementSimulation.Restore(PreparedState(state, resident, route));

        var projected = Assert.Single(
            simulation.Project().Residents,
            entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        var path = Assert.IsType<SettlementRoutePathProjection>(projected.RoutePath);

        Assert.NotNull(projected.DestinationRequest);
        Assert.Equal(SettlementTravelMode.OnFoot, path.TravelMode);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
        Assert.Equal(
            timingClass,
            Assert.Single(simulation.CaptureState().RouteConnections!).OnFootTimingClass);
    }

    [Fact]
    public void OmittedTimingClassDefaultsToUnknownAcrossStateRoundTrip()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9351)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident);
        var simulation = SettlementSimulation.Restore(PreparedState(state, resident, route));

        var captured = simulation.CaptureState();
        Assert.Equal(
            SettlementOnFootRouteTimingClass.Unknown,
            Assert.Single(captured.RouteConnections!).OnFootTimingClass);

        var decoded = SettlementStateJson.Deserialize(SettlementStateJson.Serialize(captured));
        Assert.Equal(
            SettlementOnFootRouteTimingClass.Unknown,
            Assert.Single(decoded.RouteConnections!).OnFootTimingClass);

        var restored = SettlementSimulation.Restore(decoded);
        var restoredResident = Assert.Single(
            restored.Project().Residents,
            entry => entry.Id == resident.Id);
        Assert.NotNull(restoredResident.RoutePath);
    }

    [Fact]
    public void ExplicitBaselineTimingClassSurvivesStateRoundTripWithoutStartingTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9352)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident) with
        {
            OnFootTimingClass = SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
        };
        var simulation = SettlementSimulation.Restore(PreparedState(state, resident, route));
        var captured = simulation.CaptureState();

        var decoded = SettlementStateJson.Deserialize(SettlementStateJson.Serialize(captured));
        var decodedRoute = Assert.Single(decoded.RouteConnections!);
        Assert.Equal(
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            decodedRoute.OnFootTimingClass);

        var restored = SettlementSimulation.Restore(decoded);
        var projected = Assert.Single(
            restored.Project().Residents,
            entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.NotNull(projected.RoutePath);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void RestoreRejectsUnknownOnFootRouteTimingClass()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9353)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident) with
        {
            OnFootTimingClass = (SettlementOnFootRouteTimingClass)999,
        };

        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(PreparedState(state, resident, route)));
    }

    private static SettlementState PreparedState(
        SettlementState state,
        ResidentState resident,
        SettlementRouteConnectionState route)
    {
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            30,
            "fixture.route-timing-task",
            "fixture:test-route-timing-authority",
            new SimulationTime(0),
            workplace);

        return state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id
                    ? entry with { SelectedTask = task }
                    : entry)
                .ToArray(),
            RouteConnections = [route],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(
                    resident.Id,
                    [route.ConnectionId]),
            ],
        };
    }

    private static SettlementRouteConnectionState Route(
        SettlementState state,
        ResidentState resident)
    {
        var household = Assert.Single(
            state.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(
            SettlementPlaceKind.Home,
            household.HomeId);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);

        return new SettlementRouteConnectionState(
            1,
            home,
            workplace,
            500,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            "fixture:test-route-timing-authority",
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot]);
    }
}
