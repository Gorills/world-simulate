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

            _knownRouteConnectionIdsByResident.TryGetValue(
                resident.Id,
                out var knownRouteConnectionIds);
            resident.Location = BeginSelectedTaskTravelForActor(
                resident.Location,
                resident.SelectedTask,
                knownRouteConnectionIds,
                resident.OnFootCapability,
                resident.OnFootCarriedLoad,
                Time);
        }
    }
}
