using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3TravelPlanStateTests
{
    [Fact]
    public void PlannedTravelSnapshotRoundTripsTaskRouteModeAndDeparture()
    {
        var fixture = Prepare();
        var plan = new SettlementTravelPlanState(
            fixture.Task.TaskId,
            new SimulationTime(0),
            [1, 2],
            SettlementTravelMode.OnFoot);
        var simulation = RestoreWithPlan(fixture, plan);

        var projected = Assert.Single(
            simulation.Project().Residents,
            entry => entry.Id == fixture.Resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        var travel = Assert.IsType<SettlementTravelProgressState>(location.Travel);
        var actualPlan = Assert.IsType<SettlementTravelPlanState>(travel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);
        Assert.Equal(fixture.Home, location.CurrentPlace);
        Assert.Equal(fixture.Workplace, location.DestinationPlace);
        Assert.Equal(fixture.Task.TaskId, actualPlan.TaskId);
        Assert.Equal(new SimulationTime(0), actualPlan.DepartedAt);
        Assert.Equal(new long[] { 1, 2 }, actualPlan.ConnectionIds);
        Assert.Equal(SettlementTravelMode.OnFoot, actualPlan.TravelMode);
        Assert.Null(projected.RoutePath);

        var json = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        var restoredResident = Assert.Single(
            restored.Project().Residents,
            entry => entry.Id == fixture.Resident.Id);
        var restoredLocation = Assert.IsType<SettlementActorLocationProjection>(restoredResident.Location);
        var restoredTravel = Assert.IsType<SettlementTravelProgressState>(restoredLocation.Travel);
        var restoredPlan = Assert.IsType<SettlementTravelPlanState>(restoredTravel.Plan);

        Assert.Equal(actualPlan.TaskId, restoredPlan.TaskId);
        Assert.Equal(actualPlan.DepartedAt, restoredPlan.DepartedAt);
        Assert.Equal(actualPlan.ConnectionIds.ToArray(), restoredPlan.ConnectionIds);
        Assert.Equal(actualPlan.TravelMode, restoredPlan.TravelMode);
        Assert.Equal(travel.DurationMilliseconds, restoredTravel.DurationMilliseconds);
        Assert.Equal(travel.ElapsedMilliseconds, restoredTravel.ElapsedMilliseconds);
    }

    [Fact]
    public void RestoreRejectsStructurallyInvalidTravelPlanSnapshot()
    {
        var fixture = Prepare();
        var valid = new SettlementTravelPlanState(
            fixture.Task.TaskId,
            new SimulationTime(0),
            [1, 2],
            SettlementTravelMode.OnFoot);

        Assert.Throws<InvalidOperationException>(() =>
            RestoreWithPlan(fixture, valid with { TaskId = 0 }));
        Assert.Throws<InvalidOperationException>(() =>
            RestoreWithPlan(fixture, valid with { ConnectionIds = [] }));
        Assert.Throws<InvalidOperationException>(() =>
            RestoreWithPlan(fixture, valid with { ConnectionIds = [1, 1] }));
        Assert.Throws<InvalidOperationException>(() =>
            RestoreWithPlan(fixture, valid with { TravelMode = (SettlementTravelMode)999 }));
    }

    private static TravelFixture Prepare()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9340)).CaptureState();
        var resident = state.Residents[0];
        var household = Assert.Single(
            state.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            41,
            "fixture.travel-plan-task",
            "fixture:test-travel-plan-state",
            new SimulationTime(0),
            workplace);
        return new TravelFixture(state, resident, home, workplace, task);
    }

    private static SettlementSimulation RestoreWithPlan(
        TravelFixture fixture,
        SettlementTravelPlanState plan)
    {
        var travelling = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            fixture.Home,
            fixture.Workplace,
            new SettlementTravelProgressState(
                DurationMilliseconds: 600_000,
                ElapsedMilliseconds: 120_000,
                Plan: plan));
        var residents = fixture.State.Residents
            .Select(entry => entry.Id == fixture.Resident.Id
                ? entry with
                {
                    Location = travelling,
                    SelectedTask = fixture.Task,
                }
                : entry)
            .ToArray();

        return SettlementSimulation.Restore(fixture.State with
        {
            Residents = residents,
            RouteConnections =
            [
                Route(1, fixture.Home, SettlementPlaceRef.Settlement, 150),
                Route(2, SettlementPlaceRef.Settlement, fixture.Workplace, 200),
            ],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(fixture.Resident.Id, [1, 2]),
            ],
        });
    }

    private static SettlementRouteConnectionState Route(
        long connectionId,
        SettlementPlaceRef first,
        SettlementPlaceRef second,
        long distanceMeters) =>
        new(
            connectionId,
            first,
            second,
            distanceMeters,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            "fixture:test-travel-plan-state",
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot]);

    private sealed record TravelFixture(
        SettlementState State,
        ResidentState Resident,
        SettlementPlaceRef Home,
        SettlementPlaceRef Workplace,
        SettlementSelectedTaskState Task);
}
