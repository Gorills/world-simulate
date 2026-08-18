using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3OnFootTravelDurationProjectionTests
{
    private const string CapabilityProvenance = "fixture:test-duration-capability";
    private const string LoadProvenance = "fixture:test-duration-load";
    private const string RouteProvenance = "fixture:test-duration-route";

    [Theory]
    [InlineData(1, 715)]
    [InlineData(7, 5_000)]
    [InlineData(300, 214_286)]
    [InlineData(420, 300_000)]
    public void BaselineDurationRulesUseAcceptedCeilingFormula(
        long distanceMeters,
        long expectedDurationMilliseconds)
    {
        Assert.Equal(
            expectedDurationMilliseconds,
            SettlementOnFootTravelDurationRules.CalculateBaselineDurationMilliseconds(
                distanceMeters));
    }

    [Fact]
    public void ApplicableContinuousRouteProjectsDurationWithoutStartingTravelAcrossSaveLoad()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9390)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 300));

        var projected = FindResident(simulation, resident.Id);
        var routePath = Assert.IsType<SettlementRoutePathProjection>(projected.RoutePath);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);
        var durationPlan = Assert.IsType<SettlementTravelDurationPlanProjection>(
            projected.TravelDurationPlan);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Applicable, applicability.Decision);
        Assert.Equal(routePath.TaskId, durationPlan.TaskId);
        Assert.Equal(routePath.ConnectionIds.ToArray(), durationPlan.ConnectionIds.ToArray());
        Assert.Equal(SettlementTravelMode.OnFoot, durationPlan.TravelMode);
        Assert.Equal(214_286, durationPlan.DurationMilliseconds);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);

        var decoded = SettlementStateJson.Deserialize(
            SettlementStateJson.Serialize(simulation.CaptureState()));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredProjection = FindResident(restored, resident.Id);
        var restoredPlan = Assert.IsType<SettlementTravelDurationPlanProjection>(
            restoredProjection.TravelDurationPlan);

        Assert.Equal(durationPlan.TaskId, restoredPlan.TaskId);
        Assert.Equal(durationPlan.ConnectionIds.ToArray(), restoredPlan.ConnectionIds.ToArray());
        Assert.Equal(durationPlan.TravelMode, restoredPlan.TravelMode);
        Assert.Equal(durationPlan.DurationMilliseconds, restoredPlan.DurationMilliseconds);
        Assert.Null(Assert.IsType<SettlementActorLocationProjection>(restoredProjection.Location).Travel);
    }

    [Fact]
    public void UnresolvedRouteTimingDoesNotProjectDuration()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9391)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.Unknown,
            distanceMeters: 300));

        var projected = FindResident(simulation, resident.Id);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);

        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, applicability.Decision);
        Assert.Null(projected.TravelDurationPlan);
        Assert.Null(Assert.IsType<SettlementActorLocationProjection>(projected.Location).Travel);
    }

    [Fact]
    public void RouteOutsideShortReferenceHorizonDoesNotProjectDuration()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9392)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 421));

        var projected = FindResident(simulation, resident.Id);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);

        Assert.Equal(SettlementOnFootTraversalHorizonClass.Unknown, applicability.TraversalHorizon);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, applicability.Decision);
        Assert.Null(projected.TravelDurationPlan);
        Assert.Null(Assert.IsType<SettlementActorLocationProjection>(projected.Location).Travel);
    }

    [Fact]
    public void LongerEquivalentApplicableRouteProducesLongerDuration()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9393)).CaptureState();
        var resident = state.Residents[0];
        var shorter = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 300));
        var longer = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 420));

        var shorterDuration = Assert.IsType<SettlementTravelDurationPlanProjection>(
            FindResident(shorter, resident.Id).TravelDurationPlan);
        var longerDuration = Assert.IsType<SettlementTravelDurationPlanProjection>(
            FindResident(longer, resident.Id).TravelDurationPlan);

        Assert.Equal(214_286, shorterDuration.DurationMilliseconds);
        Assert.Equal(300_000, longerDuration.DurationMilliseconds);
        Assert.True(longerDuration.DurationMilliseconds > shorterDuration.DurationMilliseconds);
    }

    [Fact]
    public void ReadySelectedTaskCommitsPersistedTravelAtNextResidentEvaluation()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9394)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 300));

        Assert.NotNull(FindResident(simulation, resident.Id).TravelDurationPlan);

        simulation.AdvanceHours(1);

        var projected = FindResident(simulation, resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        var travel = Assert.IsType<SettlementTravelProgressState>(location.Travel);
        var plan = Assert.IsType<SettlementTravelPlanState>(travel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);
        Assert.Null(projected.RoutePath);
        Assert.Null(projected.OnFootTraversalApplicability);
        Assert.Null(projected.TravelDurationPlan);
        Assert.Equal(214_286, travel.DurationMilliseconds);
        Assert.Equal(0, travel.ElapsedMilliseconds);
        Assert.Equal(50, plan.TaskId);
        Assert.Equal(SettlementSimulation.HourMilliseconds, plan.DepartedAt.Milliseconds);
        Assert.Equal(new long[] { 1 }, plan.ConnectionIds.ToArray());
        Assert.Equal(SettlementTravelMode.OnFoot, plan.TravelMode);

        var decoded = SettlementStateJson.Deserialize(
            SettlementStateJson.Serialize(simulation.CaptureState()));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredProjection = FindResident(restored, resident.Id);
        var restoredLocation = Assert.IsType<SettlementActorLocationProjection>(
            restoredProjection.Location);
        var restoredTravel = Assert.IsType<SettlementTravelProgressState>(
            restoredLocation.Travel);
        var restoredPlan = Assert.IsType<SettlementTravelPlanState>(restoredTravel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, restoredLocation.Kind);
        Assert.Equal(location.CurrentPlace, restoredLocation.CurrentPlace);
        Assert.Equal(location.DestinationPlace, restoredLocation.DestinationPlace);
        Assert.Equal(travel.DurationMilliseconds, restoredTravel.DurationMilliseconds);
        Assert.Equal(travel.ElapsedMilliseconds, restoredTravel.ElapsedMilliseconds);
        Assert.Equal(plan.TaskId, restoredPlan.TaskId);
        Assert.Equal(plan.DepartedAt, restoredPlan.DepartedAt);
        Assert.Equal(plan.ConnectionIds.ToArray(), restoredPlan.ConnectionIds.ToArray());
        Assert.Equal(plan.TravelMode, restoredPlan.TravelMode);
    }

    [Fact]
    public void PlanBearingTravelPersistsSubHourProgressAndArrivesAtStoredDuration()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9396)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 300));

        simulation.AdvanceHours(1);

        var departed = FindResident(simulation, resident.Id);
        var departedLocation = Assert.IsType<SettlementActorLocationProjection>(departed.Location);
        var departedTravel = Assert.IsType<SettlementTravelProgressState>(departedLocation.Travel);
        var plan = Assert.IsType<SettlementTravelPlanState>(departedTravel.Plan);
        var hungerAtDeparture = departed.Hunger;

        var partialTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 100_000));
        simulation.AdvanceTo(partialTime);

        var partial = FindResident(simulation, resident.Id);
        var partialLocation = Assert.IsType<SettlementActorLocationProjection>(partial.Location);
        var partialTravel = Assert.IsType<SettlementTravelProgressState>(partialLocation.Travel);

        Assert.Equal(partialTime, simulation.Time);
        Assert.Equal(SettlementActorLocationKind.Travelling, partialLocation.Kind);
        Assert.Equal(100_000, partialTravel.ElapsedMilliseconds);
        Assert.Equal(214_286, partialTravel.DurationMilliseconds);
        Assert.Equal(hungerAtDeparture, partial.Hunger);

        var decoded = SettlementStateJson.Deserialize(
            SettlementStateJson.Serialize(simulation.CaptureState()));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredPartial = FindResident(restored, resident.Id);
        var restoredPartialLocation = Assert.IsType<SettlementActorLocationProjection>(
            restoredPartial.Location);
        var restoredPartialTravel = Assert.IsType<SettlementTravelProgressState>(
            restoredPartialLocation.Travel);

        Assert.Equal(100_000, restoredPartialTravel.ElapsedMilliseconds);
        Assert.NotNull(restoredPartialTravel.Plan);

        var justBeforeArrival = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 214_285));
        restored.AdvanceTo(justBeforeArrival);

        var beforeArrival = FindResident(restored, resident.Id);
        var beforeArrivalLocation = Assert.IsType<SettlementActorLocationProjection>(
            beforeArrival.Location);
        var beforeArrivalTravel = Assert.IsType<SettlementTravelProgressState>(
            beforeArrivalLocation.Travel);

        Assert.Equal(SettlementActorLocationKind.Travelling, beforeArrivalLocation.Kind);
        Assert.Equal(214_285, beforeArrivalTravel.ElapsedMilliseconds);

        var arrivalTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 214_286));
        restored.AdvanceTo(arrivalTime);

        var arrived = FindResident(restored, resident.Id);
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);

        Assert.Equal(arrivalTime, restored.Time);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(departedLocation.DestinationPlace, arrivedLocation.CurrentPlace);
        Assert.Equal(arrivedLocation.CurrentPlace, arrivedLocation.DestinationPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.Null(arrived.DestinationRequest);
        Assert.Null(arrived.RoutePath);
        Assert.Null(arrived.OnFootTraversalApplicability);
        Assert.Null(arrived.TravelDurationPlan);
    }

    [Fact]
    public void UnresolvedTraversalDoesNotDepartAtResidentEvaluation()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9395)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.Unknown,
            distanceMeters: 300));

        simulation.AdvanceHours(1);

        var projected = FindResident(simulation, resident.Id);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, applicability.Decision);
        Assert.Null(projected.TravelDurationPlan);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);
    }

    private static ResidentProjection FindResident(
        SettlementSimulation simulation,
        EntityId residentId) =>
        Assert.Single(simulation.Project().Residents, entry => entry.Id == residentId);

    private static SettlementState PreparedState(
        SettlementState state,
        ResidentState resident,
        SettlementOnFootRouteTimingClass timingClass,
        long distanceMeters)
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
        var task = new SettlementSelectedTaskState(
            50,
            "fixture.travel-duration-task",
            "fixture:test-travel-duration",
            new SimulationTime(0),
            workplace);
        var preparedResident = resident with
        {
            SelectedTask = task,
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var route = new SettlementRouteConnectionState(
            1,
            home,
            workplace,
            distanceMeters,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            RouteProvenance,
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot],
            OnFootTimingClass: timingClass);

        return state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
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
}
