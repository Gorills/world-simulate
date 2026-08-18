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

    [Fact]
    public void ProductionProjectionKeepsUnmodeledDelayAndHorizonUnresolvedAcrossSaveLoad()
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
        Assert.Equal(SettlementOnFootTraversalHorizonClass.Unknown, applicability.TraversalHorizon);
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
    public void ExplicitNonBaselineRouteProducesNotApplicableBeforeDelayOrHorizonExist()
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
        Assert.Equal(SettlementOnFootTraversalHorizonClass.Unknown, applicability.TraversalHorizon);
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
        SettlementOnFootRouteTimingClass timingClass)
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
            300,
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
