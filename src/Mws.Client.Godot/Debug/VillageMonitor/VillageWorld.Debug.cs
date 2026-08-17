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

            var motion = view.CaptureDebugMotion();
            residents.Add(new VillageDebugResidentSnapshot(
                resident.Id,
                resident.Name,
                resident.Activity,
                resident.Profession,
                resident.Hunger,
                resident.Energy,
                resident.WorkplaceName,
                resident.HomeId,
                view.Position,
                motion.Destination,
                motion.Route,
                !motion.HasTarget || motion.Activity == resident.Activity));
        }

        return new VillageDebugSnapshot(
            projection.Day,
            projection.Hour,
            _player.Position,
            residents);
    }
}
