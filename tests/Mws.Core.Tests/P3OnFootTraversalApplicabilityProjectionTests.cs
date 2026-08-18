using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3OnFootTraversalApplicabilityProjectionTests
{
    private const string CapabilityProvenance = "fixture:test-traversal-capability";
    private const string LoadProvenance = "fixture:test-traversal-load";
    private const string RouteProvenance = "fixture:test-traversal-route";

    [Fact]
    public void RulesReturnApplicableOnlyWhenEveryDimensionExplicitlyMatchesBaseline()
    {
        var applicable = SettlementOnFootTraversalApplicabilityRules.Evaluate(
            SettlementOnFootActorCapabilityClass.BaselineCompatible,
            SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            SettlementOnFootTraversalDelayClass.NoMaterialDelay,
            SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible);
        var unresolved = SettlementOnFootTraversalApplicabilityRules.Evaluate(
            SettlementOnFootActorCapabilityClass.BaselineCompatible,
            SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            SettlementOnFootTraversalDelayClass.NoMaterialDelay,
            SettlementOnFootTraversalHorizonClass.Unknown);

        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Applicable, applicable);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, unresolved);
    }

    [Fact]
    public void RulesPreferExplicitIncompatibilityOverOtherUnknownDimensions()
    {
        var decision = SettlementOnFootTraversalApplicabilityRules.Evaluate(
            SettlementOnFootActorCapabilityClass.Unknown,
            SettlementOnFootCarriedLoadClass.MaterialLoadPresent,
            SettlementOnFootRouteTimingClass.Unknown,
            SettlementOnFootTraversalDelayClass.Unknown,
            SettlementOnFootTraversalHorizonClass.Unknown);

        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.NotApplicable, decision);
    }

    [Fact]
    public void RulesRejectUnknownEnumerationValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SettlementOnFootTraversalApplicabilityRules.Evaluate(
                (SettlementOnFootActorCapabilityClass)999,
                SettlementOnFootCarriedLoadClass.NoMaterialLoad,
                SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
                SettlementOnFootTraversalDelayClass.NoMaterialDelay,
                SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible));
    }

    [Theory]
    [InlineData(419, SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible)]
    [InlineData(420, SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible)]
    [InlineData(421, SettlementOnFootTraversalHorizonClass.Unknown)]
    [InlineData(1_000, SettlementOnFootTraversalHorizonClass.Unknown)]
    [InlineData(2_519, SettlementOnFootTraversalHorizonClass.Unknown)]
    [InlineData(2_520, SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant)]
    [InlineData(2_521, SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant)]
    public void HorizonRulesApplyAcceptedShortAndProlongedBounds(
        long totalDistanceMeters,
        SettlementOnFootTraversalHorizonClass expected)
    {
        var actual = SettlementOnFootTraversalHorizonRules.ClassifyReferenceHorizon(
            totalDistanceMeters);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HorizonRulesRejectNonPositiveDistance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SettlementOnFootTraversalHorizonRules.ClassifyReferenceHorizon(0));
    }

    [Fact]
    public void ProductionProjectionDerivesShortHorizonButKeepsUnmodeledDelayUnresolvedAcrossSaveLoad()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9380)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed));

        var projected = FindResident(simulation, resident.Id);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.NotNull(projected.RoutePath);
        Assert.Equal(SettlementOnFootActorCapabilityClass.BaselineCompatible, applicability.ActorCapability);
        Assert.Equal(SettlementOnFootCarriedLoadClass.NoMaterialLoad, applicability.CarriedLoad);
        Assert.Equal(
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            applicability.RouteTiming);
        Assert.Equal(SettlementOnFootTraversalDelayClass.Unknown, applicability.TraversalDelay);
        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible,
            applicability.TraversalHorizon);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, applicability.Decision);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);

        var decoded = SettlementStateJson.Deserialize(
            SettlementStateJson.Serialize(simulation.CaptureState()));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredApplicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(restored, resident.Id).OnFootTraversalApplicability);

        Assert.Equal(applicability, restoredApplicability);
    }

    [Fact]
    public void ProductionProjectionRecomputesHorizonWhenPathExtentChanges()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9384)).CaptureState();
        var resident = state.Residents[0];
        var shortSimulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 420));
        var middleSimulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 421));

        var shortApplicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(shortSimulation, resident.Id).OnFootTraversalApplicability);
        var middleApplicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(middleSimulation, resident.Id).OnFootTraversalApplicability);

        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible,
            shortApplicability.TraversalHorizon);
        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.Unknown,
            middleApplicability.TraversalHorizon);
        Assert.Equal(
            SettlementOnFootTraversalApplicabilityDecision.Unresolved,
            shortApplicability.Decision);
        Assert.Equal(
            SettlementOnFootTraversalApplicabilityDecision.Unresolved,
            middleApplicability.Decision);
    }

    [Fact]
    public void ShortHorizonDoesNotOverrideUnknownActorCapability()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9385)).CaptureState();
        var resident = state.Residents[0];
        var prepared = PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed,
            distanceMeters: 420);
        var simulation = SettlementSimulation.Restore(prepared with
        {
            Residents = prepared.Residents
                .Select(entry => entry.Id == resident.Id
                    ? entry with
                    {
                        OnFootCapability = SettlementOnFootActorCapabilityClass.Unknown,
                        OnFootCapabilityProvenanceReference = null,
                        IsOnFootCapabilityFixture = false,
                    }
                    : entry)
                .ToArray(),
        });

        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(simulation, resident.Id).OnFootTraversalApplicability);

        Assert.Equal(SettlementOnFootActorCapabilityClass.Unknown, applicability.ActorCapability);
        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible,
            applicability.TraversalHorizon);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.Unresolved, applicability.Decision);
    }

    [Fact]
    public void ProductionProjectionUsesAuthoritativeMultiEdgeExtentForProlongedHorizonWithoutStartingTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9383)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedMultiEdgeProlongedState(state, resident));

        var projected = FindResident(simulation, resident.Id);
        var routePath = Assert.IsType<SettlementRoutePathProjection>(projected.RoutePath);
        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            projected.OnFootTraversalApplicability);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(new long[] { 1, 2 }, routePath.ConnectionIds);
        Assert.Equal(2_520, routePath.TotalDistanceMeters);
        Assert.Equal(SettlementOnFootTraversalDelayClass.Unknown, applicability.TraversalDelay);
        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant,
            applicability.TraversalHorizon);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.NotApplicable, applicability.Decision);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Null(location.Travel);

        var decoded = SettlementStateJson.Deserialize(
            SettlementStateJson.Serialize(simulation.CaptureState()));
        var restored = SettlementSimulation.Restore(decoded);
        var restoredApplicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(restored, resident.Id).OnFootTraversalApplicability);

        Assert.Equal(applicability, restoredApplicability);
    }

    [Fact]
    public void ExplicitNonBaselineRouteProducesNotApplicableBeforeDelayExists()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9381)).CaptureState();
        var resident = state.Residents[0];
        var simulation = SettlementSimulation.Restore(PreparedState(
            state,
            resident,
            SettlementOnFootRouteTimingClass.NonBaseline));

        var applicability = Assert.IsType<SettlementOnFootTraversalApplicabilityProjection>(
            FindResident(simulation, resident.Id).OnFootTraversalApplicability);

        Assert.Equal(SettlementOnFootRouteTimingClass.NonBaseline, applicability.RouteTiming);
        Assert.Equal(SettlementOnFootTraversalDelayClass.Unknown, applicability.TraversalDelay);
        Assert.Equal(
            SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible,
            applicability.TraversalHorizon);
        Assert.Equal(SettlementOnFootTraversalApplicabilityDecision.NotApplicable, applicability.Decision);
    }

    [Fact]
    public void MissingRoutePathDoesNotInventTraversalApplicability()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9382));
        var projected = simulation.Project().Residents[0];

        Assert.Null(projected.RoutePath);
        Assert.Null(projected.OnFootTraversalApplicability);
    }

    private static ResidentProjection FindResident(
        SettlementSimulation simulation,
        EntityId residentId) =>
        Assert.Single(simulation.Project().Residents, entry => entry.Id == residentId);

    private static SettlementState PreparedState(
        SettlementState state,
        ResidentState resident,
        SettlementOnFootRouteTimingClass timingClass,
        long distanceMeters = 300)
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
            40,
            "fixture.traversal-applicability-task",
            "fixture:test-traversal-applicability",
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

    private static SettlementState PreparedMultiEdgeProlongedState(
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
        var task = new SettlementSelectedTaskState(
            41,
            "fixture.traversal-horizon-task",
            "fixture:test-traversal-horizon",
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
        var firstRoute = new SettlementRouteConnectionState(
            1,
            home,
            SettlementPlaceRef.Settlement,
            1_260,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            RouteProvenance,
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot],
            OnFootTimingClass: SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed);
        var secondRoute = new SettlementRouteConnectionState(
            2,
            SettlementPlaceRef.Settlement,
            workplace,
            1_260,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            RouteProvenance,
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot],
            OnFootTimingClass: SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed);

        return state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
                .ToArray(),
            RouteConnections = [firstRoute, secondRoute],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(
                    resident.Id,
                    [firstRoute.ConnectionId, secondRoute.ConnectionId]),
            ],
        };
    }
}
