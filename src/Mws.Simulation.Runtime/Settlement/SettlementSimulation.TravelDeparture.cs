using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void BeginReadySelectedTaskTravelDepartures()
    {
        foreach (var resident in _residents)
        {
            if (resident.SelectedTask is null
                || resident.Location.Kind != SettlementActorLocationKind.AtPlace)
            {
                continue;
            }

            var destinationRequest = ProjectDestinationRequest(resident);
            var routePath = ProjectRoutePath(resident, destinationRequest);
            var applicability = ProjectOnFootTraversalApplicability(resident, routePath);
            var durationPlan = ProjectTravelDurationPlan(routePath, applicability);
            if (routePath is null || durationPlan is null)
            {
                continue;
            }

            var persistedPlan = new SettlementTravelPlanState(
                durationPlan.TaskId,
                Time,
                durationPlan.ConnectionIds.ToArray(),
                durationPlan.TravelMode);
            resident.Location = SettlementSemanticLocation.BeginTravel(
                resident.Location,
                routePath.Destination,
                durationPlan.DurationMilliseconds,
                persistedPlan);
        }
    }
}
