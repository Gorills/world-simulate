using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3RouteInterruptionTests
{
    [Fact]
    public void BlockedActiveRouteFreezesPersistedProgressAndReopeningResumesSamePlan()
    {
        const long durationMilliseconds = 214_286;
        const long initialElapsedMilliseconds = 100_000;
        const long openProgressMilliseconds = 50_000;
        const long blockedElapsedMilliseconds = 300_000;

        var state = SettlementSimulation.CreateDefault(new WorldSeed(9350)).CaptureState() with
        {
            Time = new SimulationTime(SettlementSimulation.HourMilliseconds),
        };
        var resident = state.Residents[0];
        var household = Assert.Single(
            state.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            51,
            "fixture.route-interruption-task",
            "fixture:test-route-interruption",
            new SimulationTime(0),
            workplace);
        var plan = new SettlementTravelPlanState(
            task.TaskId,
            new SimulationTime(SettlementSimulation.HourMilliseconds - initialElapsedMilliseconds),
            [1],
            SettlementTravelMode.OnFoot);
        var travelling = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            home,
            workplace,
            new SettlementTravelProgressState(
                durationMilliseconds,
                initialElapsedMilliseconds,
                plan));
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id
                ? entry with
                {
                    Location = travelling,
                    SelectedTask = task,
                }
                : entry)
            .ToArray();
        var route = Route(1, home, workplace, 300);
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = residents,
            RouteConnections = [route],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [1]),
            ],
        });

        simulation.AdvanceTo(simulation.Time.AddMilliseconds(openProgressMilliseconds));
        var beforeBlock = ProjectResident(simulation, resident.Id);
        var beforeBlockLocation = Assert.IsType<SettlementActorLocationProjection>(beforeBlock.Location);
        var beforeBlockTravel = Assert.IsType<SettlementTravelProgressState>(beforeBlockLocation.Travel);
        Assert.Equal(initialElapsedMilliseconds + openProgressMilliseconds, beforeBlockTravel.ElapsedMilliseconds);

        var blockedState = simulation.CaptureState();
        var blocked = SettlementSimulation.Restore(blockedState with
        {
            RouteConnections = blockedState.RouteConnections!
                .Select(connection => connection.ConnectionId == 1
                    ? connection with { PhysicalState = SettlementRoutePhysicalState.Blocked }
                    : connection)
                .ToArray(),
        });
        var blockedAt = blocked.Time;
        blocked.AdvanceTo(blockedAt.AddMilliseconds(blockedElapsedMilliseconds));

        var paused = ProjectResident(blocked, resident.Id);
        var pausedLocation = Assert.IsType<SettlementActorLocationProjection>(paused.Location);
        var pausedTravel = Assert.IsType<SettlementTravelProgressState>(pausedLocation.Travel);
        var pausedPlan = Assert.IsType<SettlementTravelPlanState>(pausedTravel.Plan);
        Assert.Equal(blockedAt.AddMilliseconds(blockedElapsedMilliseconds), blocked.Time);
        Assert.Equal(SettlementActorLocationKind.Travelling, pausedLocation.Kind);
        Assert.Equal(home, pausedLocation.CurrentPlace);
        Assert.Equal(workplace, pausedLocation.DestinationPlace);
        Assert.Equal(initialElapsedMilliseconds + openProgressMilliseconds, pausedTravel.ElapsedMilliseconds);
        Assert.Equal(plan.TaskId, pausedPlan.TaskId);
        Assert.Equal(plan.DepartedAt, pausedPlan.DepartedAt);
        Assert.Equal(plan.ConnectionIds.ToArray(), pausedPlan.ConnectionIds);
        Assert.Equal(plan.TravelMode, pausedPlan.TravelMode);
        Assert.Equal(task.TaskId, Assert.IsType<SettlementSelectedTaskProjection>(paused.SelectedTask).TaskId);

        var pausedJson = SettlementStateJson.Serialize(blocked.CaptureState());
        var restoredPaused = SettlementSimulation.Restore(SettlementStateJson.Deserialize(pausedJson));
        var restoredResident = ProjectResident(restoredPaused, resident.Id);
        var restoredLocation = Assert.IsType<SettlementActorLocationProjection>(restoredResident.Location);
        var restoredTravel = Assert.IsType<SettlementTravelProgressState>(restoredLocation.Travel);
        var restoredPlan = Assert.IsType<SettlementTravelPlanState>(restoredTravel.Plan);
        Assert.Equal(pausedTravel.DurationMilliseconds, restoredTravel.DurationMilliseconds);
        Assert.Equal(pausedTravel.ElapsedMilliseconds, restoredTravel.ElapsedMilliseconds);
        Assert.Equal(pausedPlan.TaskId, restoredPlan.TaskId);
        Assert.Equal(pausedPlan.DepartedAt, restoredPlan.DepartedAt);
        Assert.Equal(pausedPlan.ConnectionIds.ToArray(), restoredPlan.ConnectionIds);
        Assert.Equal(pausedPlan.TravelMode, restoredPlan.TravelMode);

        var reopenedState = restoredPaused.CaptureState();
        var reopened = SettlementSimulation.Restore(reopenedState with
        {
            RouteConnections = reopenedState.RouteConnections!
                .Select(connection => connection.ConnectionId == 1
                    ? connection with { PhysicalState = SettlementRoutePhysicalState.Passable }
                    : connection)
                .ToArray(),
        });
        var remainingMilliseconds = checked(
            durationMilliseconds - restoredTravel.ElapsedMilliseconds);
        reopened.AdvanceTo(reopened.Time.AddMilliseconds(remainingMilliseconds));

        var arrived = ProjectResident(reopened, resident.Id);
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(workplace, arrivedLocation.CurrentPlace);
        Assert.Equal(workplace, arrivedLocation.DestinationPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.Equal(task.TaskId, Assert.IsType<SettlementSelectedTaskProjection>(arrived.SelectedTask).TaskId);
    }

    private static ResidentProjection ProjectResident(
        SettlementSimulation simulation,
        EntityId residentId) =>
        Assert.Single(simulation.Project().Residents, entry => entry.Id == residentId);

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
            "fixture:test-route-interruption",
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot]);
}
