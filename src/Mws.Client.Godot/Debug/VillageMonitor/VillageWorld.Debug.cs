using Mws.Client.Godot.Debug.VillageMonitor;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

public partial class VillageWorld
{
    internal VillageDebugSnapshot CaptureDebugSnapshot(SettlementProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var residents = new List<VillageDebugResidentSnapshot>(projection.Residents.Count);
        foreach (var resident in projection.Residents)
        {
            if (!_residentViews.TryGetValue(resident.Id.Value, out var view))
            {
                continue;
            }

            var location = resident.Location
                ?? throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has no authoritative semantic location for debug presentation.");
            var expectedPosition = VillageResidentPlacement.Resolve(resident, projection);
            var destination = VillageResidentPlacement.ResolvePlace(
                location.DestinationPlace,
                projection);
            var travel = location.Travel;

            residents.Add(new VillageDebugResidentSnapshot(
                resident.Id,
                resident.Name,
                resident.Activity,
                resident.Hunger,
                resident.Energy,
                view.Position,
                destination,
                location.Kind,
                location.DestinationPlace,
                travel?.ElapsedMilliseconds,
                travel?.DurationMilliseconds,
                view.Position.DistanceTo(expectedPosition) <= 0.001f));
        }

        return new VillageDebugSnapshot(
            projection.Day,
            projection.Hour,
            _player.Position,
            residents);
    }
}
