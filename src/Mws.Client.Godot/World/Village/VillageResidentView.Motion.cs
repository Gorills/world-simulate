using Godot;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal sealed partial class VillageResidentView
{
    private const float WalkSpeedMetersPerSecond = 1.65f;
    private const float ArrivalDistanceMeters = 0.08f;
    private const float FacingResponsiveness = 9.0f;

    private readonly Queue<Vector3> _route = new();
    private Vector3 _routeDestination;
    private ResidentActivity _routeActivity;
    private bool _hasRouteTarget;

    public override void _Process(double delta)
    {
        if (_route.Count == 0)
        {
            return;
        }

        while (_route.Count > 0 && FlatDistance(Position, _route.Peek()) <= ArrivalDistanceMeters)
        {
            var reached = _route.Dequeue();
            Position = new Vector3(reached.X, Position.Y, reached.Z);
        }

        if (_route.Count == 0)
        {
            return;
        }

        var target = _route.Peek();
        var offset = new Vector3(target.X - Position.X, 0.0f, target.Z - Position.Z);
        var distance = offset.Length();
        if (distance <= 0.0001f)
        {
            return;
        }

        var step = Math.Min(distance, WalkSpeedMetersPerSecond * (float)delta);
        Position += (offset / distance) * step;

        var targetYaw = Mathf.Atan2(-offset.X, -offset.Z);
        var rotation = Rotation;
        rotation.Y = Mathf.LerpAngle(
            rotation.Y,
            targetYaw,
            Mathf.Clamp(FacingResponsiveness * (float)delta, 0.0f, 1.0f));
        Rotation = rotation;
    }

    internal void SetRoute(IReadOnlyList<Vector3> route, ResidentActivity activity)
    {
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count == 0)
        {
            return;
        }

        var destination = route[^1];
        if (_hasRouteTarget
            && _routeActivity == activity
            && FlatDistance(_routeDestination, destination) <= ArrivalDistanceMeters)
        {
            return;
        }

        _route.Clear();
        foreach (var point in route)
        {
            _route.Enqueue(point);
        }

        _routeDestination = destination;
        _routeActivity = activity;
        _hasRouteTarget = true;
    }

    private static float FlatDistance(Vector3 left, Vector3 right)
    {
        var x = left.X - right.X;
        var z = left.Z - right.Z;
        return MathF.Sqrt((x * x) + (z * z));
    }
}
