using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3RouteModeEncodingTests
{
    [Fact]
    public void CurrentEncodingRejectsMissingRouteModeSupport()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9340)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident, supportedModes: null);

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections = [route],
            RouteModeEncodingVersion = SettlementVersions.CurrentRouteModeEncodingVersion,
        }));
    }

    [Fact]
    public void LegacyEncodingKeepsMissingRouteModeSupportUnresolvedAcrossRoundTrip()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9341)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident, supportedModes: null);
        var prepared = state with
        {
            Residents = WithSelectedTask(state, resident),
            RouteConnections = [route],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [route.ConnectionId]),
            ],
            RouteModeEncodingVersion = SettlementVersions.LegacyRouteModeEncodingVersion,
        };

        var simulation = SettlementSimulation.Restore(prepared);
        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);

        Assert.NotNull(projected.DestinationRequest);
        Assert.Null(projected.RoutePath);

        var captured = simulation.CaptureState();
        Assert.Equal(
            SettlementVersions.LegacyRouteModeEncodingVersion,
            captured.RouteModeEncodingVersion);

        var decoded = SettlementStateJson.Deserialize(SettlementStateJson.Serialize(captured));
        Assert.Equal(
            SettlementVersions.LegacyRouteModeEncodingVersion,
            decoded.RouteModeEncodingVersion);
        var restored = SettlementSimulation.Restore(decoded);
        var restoredResident = Assert.Single(
            restored.Project().Residents,
            entry => entry.Id == resident.Id);
        Assert.Null(restoredResident.RoutePath);
    }

    [Fact]
    public void ExplicitRouteModeSupportUpgradesCapturedEncoding()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9342)).CaptureState();
        var resident = state.Residents[0];
        var route = Route(state, resident, [SettlementTravelMode.OnFoot]);
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = WithSelectedTask(state, resident),
            RouteConnections = [route],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [route.ConnectionId]),
            ],
            RouteModeEncodingVersion = SettlementVersions.LegacyRouteModeEncodingVersion,
        });

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var path = Assert.IsType<SettlementRoutePathProjection>(projected.RoutePath);
        Assert.Equal(SettlementTravelMode.OnFoot, path.TravelMode);

        var captured = simulation.CaptureState();
        Assert.Equal(
            SettlementVersions.CurrentRouteModeEncodingVersion,
            captured.RouteModeEncodingVersion);

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(captured with
        {
            RouteConnections = [route with { SupportedModes = null }],
        }));
    }

    [Fact]
    public void RestoreRejectsUnknownRouteModeEncoding()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9343)).CaptureState();

        Assert.Throws<NotSupportedException>(() => SettlementSimulation.Restore(state with
        {
            RouteModeEncodingVersion = 999,
        }));
    }

    private static SettlementRouteConnectionState Route(
        SettlementState state,
        ResidentState resident,
        IReadOnlyList<SettlementTravelMode>? supportedModes)
    {
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        return new SettlementRouteConnectionState(
            1,
            home,
            workplace,
            500,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            "fixture:test-route-mode-encoding",
            IsFixture: true,
            SupportedModes: supportedModes);
    }

    private static ResidentState[] WithSelectedTask(SettlementState state, ResidentState resident)
    {
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            20,
            "fixture.route-mode-task",
            "fixture:test-route-mode-encoding",
            new SimulationTime(0),
            workplace);
        return state.Residents
            .Select(entry => entry.Id == resident.Id ? entry with { SelectedTask = task } : entry)
            .ToArray();
    }

    private static SettlementPlaceRef ResidentHome(SettlementState state, ResidentState resident)
    {
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);
        return new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
    }
}
