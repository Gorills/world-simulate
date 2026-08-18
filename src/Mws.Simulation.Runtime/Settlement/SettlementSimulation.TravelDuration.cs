using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public static class SettlementOnFootTravelDurationRules
{
    private const long BaselineWalkingSpeedMillimetersPerSecond = 1_400;
    private const long MillimeterMillisecondsPerMeter = 1_000_000;

    public static long CalculateBaselineDurationMilliseconds(long distanceMeters)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(distanceMeters);

        var numerator = checked(distanceMeters * MillimeterMillisecondsPerMeter);
        return checked(
            (numerator + BaselineWalkingSpeedMillimetersPerSecond - 1)
            / BaselineWalkingSpeedMillimetersPerSecond);
    }
}

public sealed partial class SettlementSimulation
{
    private static SettlementTravelDurationPlanProjection? ProjectTravelDurationPlan(
        SettlementRoutePathProjection? routePath,
        SettlementOnFootTraversalApplicabilityProjection? applicability)
    {
        if (routePath is null
            || applicability is null
            || routePath.TravelMode != SettlementTravelMode.OnFoot
            || applicability.TaskId != routePath.TaskId
            || applicability.Decision
                != SettlementOnFootTraversalApplicabilityDecision.Applicable)
        {
            return null;
        }

        return new SettlementTravelDurationPlanProjection(
            routePath.TaskId,
            routePath.ConnectionIds.ToArray(),
            routePath.TravelMode,
            SettlementOnFootTravelDurationRules.CalculateBaselineDurationMilliseconds(
                routePath.TotalDistanceMeters));
    }
}
