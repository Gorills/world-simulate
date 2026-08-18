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
                || travel?.Plan is null)
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
            if (resident.Location.Kind != SettlementActorLocationKind.Travelling
                || resident.Location.Travel?.Plan is null)
            {
                continue;
            }

            resident.Location = SettlementSemanticLocation.AdvanceTravel(
                resident.Location,
                elapsedMilliseconds);
        }
    }
}
