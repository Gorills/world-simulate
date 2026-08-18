using Mws.Client.Godot.Debug.VillageMonitor;

namespace Mws.Client.Godot.World.Village;

internal sealed partial class VillageResidentView
{
    internal VillageResidentDebugMotion CaptureDebugMotion() =>
        new(
            _hasRouteTarget ? _routeDestination : Position,
            _route.ToArray(),
            _routeActivity,
            _hasRouteTarget);
}
