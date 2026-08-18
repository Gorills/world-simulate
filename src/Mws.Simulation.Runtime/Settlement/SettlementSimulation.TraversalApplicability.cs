using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public static class SettlementOnFootTraversalApplicabilityRules
{
    public static SettlementOnFootTraversalApplicabilityDecision Evaluate(
        SettlementOnFootActorCapabilityClass actorCapability,
        SettlementOnFootCarriedLoadClass carriedLoad,
        SettlementOnFootRouteTimingClass routeTiming,
        SettlementOnFootTraversalDelayClass traversalDelay,
        SettlementOnFootTraversalHorizonClass traversalHorizon)
    {
        Validate(actorCapability, carriedLoad, routeTiming, traversalDelay, traversalHorizon);

        if (actorCapability == SettlementOnFootActorCapabilityClass.NonBaseline
            || carriedLoad == SettlementOnFootCarriedLoadClass.MaterialLoadPresent
            || routeTiming == SettlementOnFootRouteTimingClass.NonBaseline
            || traversalDelay == SettlementOnFootTraversalDelayClass.MaterialDelayPresent
            || traversalHorizon == SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant)
        {
            return SettlementOnFootTraversalApplicabilityDecision.NotApplicable;
        }

        if (actorCapability == SettlementOnFootActorCapabilityClass.Unknown
            || carriedLoad == SettlementOnFootCarriedLoadClass.Unknown
            || routeTiming == SettlementOnFootRouteTimingClass.Unknown
            || traversalDelay == SettlementOnFootTraversalDelayClass.Unknown
            || traversalHorizon == SettlementOnFootTraversalHorizonClass.Unknown)
        {
            return SettlementOnFootTraversalApplicabilityDecision.Unresolved;
        }

        return SettlementOnFootTraversalApplicabilityDecision.Applicable;
    }

    private static void Validate(
        SettlementOnFootActorCapabilityClass actorCapability,
        SettlementOnFootCarriedLoadClass carriedLoad,
        SettlementOnFootRouteTimingClass routeTiming,
        SettlementOnFootTraversalDelayClass traversalDelay,
        SettlementOnFootTraversalHorizonClass traversalHorizon)
    {
        if (actorCapability is not (
            SettlementOnFootActorCapabilityClass.Unknown
            or SettlementOnFootActorCapabilityClass.BaselineCompatible
            or SettlementOnFootActorCapabilityClass.NonBaseline))
        {
            throw new ArgumentOutOfRangeException(nameof(actorCapability));
        }

        if (carriedLoad is not (
            SettlementOnFootCarriedLoadClass.Unknown
            or SettlementOnFootCarriedLoadClass.NoMaterialLoad
            or SettlementOnFootCarriedLoadClass.MaterialLoadPresent))
        {
            throw new ArgumentOutOfRangeException(nameof(carriedLoad));
        }

        if (routeTiming is not (
            SettlementOnFootRouteTimingClass.Unknown
            or SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed
            or SettlementOnFootRouteTimingClass.NonBaseline))
        {
            throw new ArgumentOutOfRangeException(nameof(routeTiming));
        }

        if (traversalDelay is not (
            SettlementOnFootTraversalDelayClass.Unknown
            or SettlementOnFootTraversalDelayClass.NoMaterialDelay
            or SettlementOnFootTraversalDelayClass.MaterialDelayPresent))
        {
            throw new ArgumentOutOfRangeException(nameof(traversalDelay));
        }

        if (traversalHorizon is not (
            SettlementOnFootTraversalHorizonClass.Unknown
            or SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible
            or SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant))
        {
            throw new ArgumentOutOfRangeException(nameof(traversalHorizon));
        }
    }
}

public static class SettlementOnFootTraversalHorizonRules
{
    private const long BaselineReferenceWalkingSpeedMillimetersPerSecond = 1_400;
    private const long BaselineShortReferenceHorizonMilliseconds = 300_000;
    private const long ProlongedReferenceHorizonMilliseconds = 1_800_000;
    private const long MillisecondsPerSecond = 1_000;
    private const long MillimetersPerMeter = 1_000;
    private const long BaselineShortReferenceDistanceMeters =
        BaselineReferenceWalkingSpeedMillimetersPerSecond
        * BaselineShortReferenceHorizonMilliseconds
        / MillisecondsPerSecond
        / MillimetersPerMeter;
    private const long ProlongedReferenceDistanceMeters =
        BaselineReferenceWalkingSpeedMillimetersPerSecond
        * ProlongedReferenceHorizonMilliseconds
        / MillisecondsPerSecond
        / MillimetersPerMeter;

    public static SettlementOnFootTraversalHorizonClass ClassifyReferenceHorizon(
        long totalDistanceMeters)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalDistanceMeters);

        if (totalDistanceMeters <= BaselineShortReferenceDistanceMeters)
        {
            return SettlementOnFootTraversalHorizonClass.BaselineShortReferenceCompatible;
        }

        return totalDistanceMeters >= ProlongedReferenceDistanceMeters
            ? SettlementOnFootTraversalHorizonClass.ProlongedOrEnduranceRelevant
            : SettlementOnFootTraversalHorizonClass.Unknown;
    }
}

public sealed partial class SettlementSimulation
{
    private SettlementOnFootTraversalApplicabilityProjection? ProjectOnFootTraversalApplicability(
        ResidentRuntimeState resident,
        SettlementRoutePathProjection? routePath)
    {
        if (routePath is null || routePath.TravelMode != SettlementTravelMode.OnFoot)
        {
            return null;
        }

        var routeTiming = AggregateOnFootRouteTiming(routePath.ConnectionIds);
        // Current P3 route projection represents only the accepted continuous ordinary
        // OnFoot profile. Process-bearing routes stay outside this profile until modeled.
        const SettlementOnFootTraversalDelayClass traversalDelay =
            SettlementOnFootTraversalDelayClass.NoMaterialDelay;
        var traversalHorizon = SettlementOnFootTraversalHorizonRules.ClassifyReferenceHorizon(
            routePath.TotalDistanceMeters);
        var decision = SettlementOnFootTraversalApplicabilityRules.Evaluate(
            resident.OnFootCapability,
            resident.OnFootCarriedLoad,
            routeTiming,
            traversalDelay,
            traversalHorizon);

        return new SettlementOnFootTraversalApplicabilityProjection(
            routePath.TaskId,
            resident.OnFootCapability,
            resident.OnFootCarriedLoad,
            routeTiming,
            traversalDelay,
            traversalHorizon,
            decision);
    }

    private SettlementOnFootRouteTimingClass AggregateOnFootRouteTiming(
        IReadOnlyList<long> connectionIds)
    {
        if (connectionIds.Count == 0)
        {
            return SettlementOnFootRouteTimingClass.Unknown;
        }

        var sawUnknown = false;
        foreach (var connectionId in connectionIds)
        {
            if (!_routeConnectionsById.TryGetValue(connectionId, out var connection))
            {
                throw new InvalidOperationException(
                    "Projected route path references a missing route connection.");
            }

            if (connection.OnFootTimingClass == SettlementOnFootRouteTimingClass.NonBaseline)
            {
                return SettlementOnFootRouteTimingClass.NonBaseline;
            }

            sawUnknown |= connection.OnFootTimingClass == SettlementOnFootRouteTimingClass.Unknown;
        }

        return sawUnknown
            ? SettlementOnFootRouteTimingClass.Unknown
            : SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed;
    }
}
