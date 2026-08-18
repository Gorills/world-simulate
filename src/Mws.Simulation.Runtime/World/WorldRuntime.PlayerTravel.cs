using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private WorldPlayerActorState? StagePlayerAdvance(SimulationTime target)
    {
        if (_player is null)
        {
            return null;
        }

        var player = _player;
        var authority = CreatePlayerTravelAuthority(player.ScopeId);
        var location = SettlementSemanticLocation.Normalize(player.Location);
        if (location.Kind == SettlementActorLocationKind.AtPlace
            && player.SelectedTask is not null)
        {
            location = authority.BeginSelectedTaskTravelForActor(
                location,
                player.SelectedTask,
                player.KnownRouteConnectionIds,
                player.OnFootCapability,
                player.OnFootCarriedLoad,
                Time);
        }

        var elapsedMilliseconds = checked(target.Milliseconds - Time.Milliseconds);
        location = authority.AdvanceTravelForActor(location, elapsedMilliseconds);
        return player with { Location = location };
    }
}
