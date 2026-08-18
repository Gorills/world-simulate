using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3DefaultTravelVerticalSliceTests
{
    [Fact]
    public void DefaultContentCarriesOneBoundedAuthoritativeOnFootSliceWithoutInventingATask()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9400));
        var state = simulation.CaptureState();
        var resident = Assert.Single(state.Residents, entry => entry.Name == "Karo");
        var household = Assert.Single(
            state.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var route = Assert.Single(state.RouteConnections!);
        var knowledge = Assert.Single(state.ResidentRouteKnowledge!);

        Assert.Null(resident.SelectedTask);
        Assert.Equal(
            SettlementOnFootActorCapabilityClass.BaselineCompatible,
            resident.OnFootCapability);
        Assert.False(resident.IsOnFootCapabilityFixture);
        Assert.False(string.IsNullOrWhiteSpace(resident.OnFootCapabilityProvenanceReference));
        Assert.Equal(
            SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            resident.OnFootCarriedLoad);
        Assert.False(resident.IsOnFootCarriedLoadFixture);
        Assert.False(string.IsNullOrWhiteSpace(resident.OnFootCarriedLoadProvenanceReference));

        Assert.Equal(home, route.FirstPlace);
        Assert.Equal(workplace, route.SecondPlace);
        Assert.Equal(300, route.DistanceMeters);
        Assert.Equal(SettlementRoutePhysicalState.Passable, route.PhysicalState);
        Assert.Equal(SettlementRoutePassageStatus.Open, route.PassageStatus);
        Assert.False(route.IsFixture);
        Assert.Equal(
            new[] { SettlementTravelMode.OnFoot },
            route.SupportedModes!.ToArray());
        Assert.Equal(
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            route.OnFootTimingClass);
        Assert.False(string.IsNullOrWhiteSpace(route.ProvenanceReference));

        Assert.Equal(resident.Id, knowledge.ResidentId);
        Assert.Equal(new[] { route.ConnectionId }, knowledge.KnownConnectionIds.ToArray());

        var projected = FindResident(simulation, resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Equal(home, location.CurrentPlace);
        Assert.Null(location.Travel);
        Assert.Null(projected.DestinationRequest);
        Assert.Null(projected.RoutePath);
        Assert.Null(projected.OnFootTraversalApplicability);
        Assert.Null(projected.TravelDurationPlan);
    }

    [Fact]
    public void ExplicitTaskUsesDefaultAuthorityThroughDepartureSaveLoadProgressAndArrival()
    {
        var baseState = SettlementSimulation.CreateDefault(new WorldSeed(9401)).CaptureState();
        var resident = Assert.Single(baseState.Residents, entry => entry.Name == "Karo");
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            70,
            "fixture.explicit-default-p3-travel-task",
            "fixture:test-default-p3-travel-task",
            new SimulationTime(0),
            workplace);
        var simulation = SettlementSimulation.Restore(baseState with
        {
            Residents = baseState.Residents
                .Select(entry => entry.Id == resident.Id
                    ? entry with { SelectedTask = task }
                    : entry)
                .ToArray(),
        });

        var ready = FindResident(simulation, resident.Id);
        var routePath = Assert.IsType<SettlementRoutePathProjection>(ready.RoutePath);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            ready.OnFootTraversalApplicability);
        var duration = Assert.IsType<SettlementTravelDurationPlanProjection>(
            ready.TravelDurationPlan);

        Assert.Equal(task.TaskId, routePath.TaskId);
        Assert.Equal(300, routePath.TotalDistanceMeters);
        Assert.Single(routePath.ConnectionIds);
        Assert.Equal(
            SettlementOnFootTraversalApplicabilityDecision.Applicable,
            applicability.Decision);
        Assert.Equal(214_286, duration.DurationMilliseconds);

        simulation.AdvanceHours(1);

        var departed = FindResident(simulation, resident.Id);
        var departedLocation = Assert.IsType<SettlementActorLocationProjection>(departed.Location);
        var departedTravel = Assert.IsType<SettlementTravelProgressState>(departedLocation.Travel);
        var plan = Assert.IsType<SettlementTravelPlanState>(departedTravel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, departedLocation.Kind);
        Assert.Equal(task.TaskId, plan.TaskId);
        Assert.Equal(SettlementSimulation.HourMilliseconds, plan.DepartedAt.Milliseconds);
        Assert.Equal(routePath.ConnectionIds.ToArray(), plan.ConnectionIds.ToArray());
        Assert.Equal(214_286, departedTravel.DurationMilliseconds);
        Assert.Equal(0, departedTravel.ElapsedMilliseconds);

        var partialTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 100_000));
        simulation.AdvanceTo(partialTime);

        var partial = FindResident(simulation, resident.Id);
        var partialLocation = Assert.IsType<SettlementActorLocationProjection>(partial.Location);
        var partialTravel = Assert.IsType<SettlementTravelProgressState>(partialLocation.Travel);

        Assert.Equal(SettlementActorLocationKind.Travelling, partialLocation.Kind);
        Assert.Equal(100_000, partialTravel.ElapsedMilliseconds);

        var restored = SettlementSimulation.Restore(
            SettlementStateJson.Deserialize(
                SettlementStateJson.Serialize(simulation.CaptureState())));
        var restoredPartial = FindResident(restored, resident.Id);
        var restoredPartialLocation = Assert.IsType<SettlementActorLocationProjection>(
            restoredPartial.Location);
        var restoredTravel = Assert.IsType<SettlementTravelProgressState>(
            restoredPartialLocation.Travel);

        Assert.Equal(100_000, restoredTravel.ElapsedMilliseconds);
        Assert.NotNull(restoredTravel.Plan);

        var arrivalTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 214_286));
        restored.AdvanceTo(arrivalTime);

        var arrived = FindResident(restored, resident.Id);
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);

        Assert.Equal(arrivalTime, restored.Time);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(workplace, arrivedLocation.CurrentPlace);
        Assert.Equal(workplace, arrivedLocation.DestinationPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.Null(arrived.DestinationRequest);
        Assert.Null(arrived.RoutePath);
        Assert.Null(arrived.OnFootTraversalApplicability);
        Assert.Null(arrived.TravelDurationPlan);
    }

    private static ResidentProjection FindResident(
        SettlementSimulation simulation,
        EntityId residentId) =>
        Assert.Single(simulation.Project().Residents, entry => entry.Id == residentId);
}
