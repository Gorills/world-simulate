using Godot;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Debug.VillageMonitor;

internal sealed record VillageDebugResidentSnapshot(
    EntityId Id,
    string Name,
    ResidentActivity Activity,
    int Hunger,
    int Energy,
    Vector3 Position,
    Vector3 Destination,
    SettlementActorLocationKind LocationKind,
    SettlementPlaceRef DestinationPlace,
    long? TravelElapsedMilliseconds,
    long? TravelDurationMilliseconds,
    bool PlacementMatchesAuthority)
{
    internal float DistanceToDestination
    {
        get
        {
            var x = Position.X - Destination.X;
            var z = Position.Z - Destination.Z;
            return MathF.Sqrt((x * x) + (z * z));
        }
    }
}

internal sealed record VillageDebugSnapshot(
    int Day,
    int Hour,
    Vector3 PlayerPosition,
    IReadOnlyList<VillageDebugResidentSnapshot> Residents);
