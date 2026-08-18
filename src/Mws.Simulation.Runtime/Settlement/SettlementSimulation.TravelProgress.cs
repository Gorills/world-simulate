using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private SimulationTime NextPlanBearingTravelBoundary(SimulationTime upperBound)
    {
        var nextMilliseconds = upperBound.Milliseconds;
        foreach (var resident in _residents)
        {
            var travel = resident.Location.Travel;
            if (resident.Location.Kind != SettlementActorLocationKind.Travelling
                || travel?.Plan is null
                || !IsActiveTravelPlanTraversable(travel))
            {
                continue;
            }

            var remainingMilliseconds = checked(
                travel.DurationMilliseconds - travel.ElapsedMilliseconds);
            var completionMilliseconds = checked(
                Time.Milliseconds + remainingMilliseconds);
            if (completionMilliseconds < nextMilliseconds)
            {
                nextMilliseconds = completionMilliseconds;
            }
        }

        return new SimulationTime(nextMilliseconds);
    }

    private void AdvancePlanBearingTravelProgress(long elapsedMilliseconds)
    {
        if (elapsedMilliseconds <= 0)
        {
            return;
        }

        foreach (var resident in _residents)
        {
            var travel = resident.Location.Travel;
            if (resident.Location.Kind != SettlementActorLocationKind.Travelling
                || travel?.Plan is null
                || !IsActiveTravelPlanTraversable(travel))
            {
                continue;
            }

            resident.Location = SettlementSemanticLocation.AdvanceTravel(
                resident.Location,
                elapsedMilliseconds);
        }
    }

    private bool IsActiveTravelPlanTraversable(SettlementTravelProgressState travel)
    {
        var plan = travel.Plan
            ?? throw new InvalidOperationException(
                "Active travel plan availability requires a persisted plan.");

        foreach (var connectionId in plan.ConnectionIds)
        {
            if (!_routeConnectionsById.TryGetValue(connectionId, out var connection)
                || connection.PhysicalState != SettlementRoutePhysicalState.Passable
                || connection.PassageStatus != SettlementRoutePassageStatus.Open
                || connection.SupportedModes?.Contains(plan.TravelMode) != true)
            {
                // The plan stays authoritative while unavailable. Without an accepted
                // reroute/reconsideration decision, preserve its progress instead of
                // teleporting, silently completing, or inventing another route.
                return false;
            }
        }

        return true;
    }
}
