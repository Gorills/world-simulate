using Godot;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Debug.VillageMonitor;

internal sealed record VillageDebugResidentSnapshot(
    EntityId Id,
    string Name,
    ResidentActivity Activity,
    ResidentProfession Profession,
    int Hunger,
    int Energy,
    string WorkplaceName,
    EntityId HomeId,
    Vector3 Position,
    Vector3 Destination,
    IReadOnlyList<Vector3> Route,
    bool RouteMatchesActivity)
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

internal sealed record VillageResidentDebugMotion(
    Vector3 Destination,
    IReadOnlyList<Vector3> Route,
    ResidentActivity Activity,
    bool HasTarget);
