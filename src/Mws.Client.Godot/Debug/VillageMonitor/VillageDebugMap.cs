using Godot;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Debug.VillageMonitor;

public partial class VillageDebugMap : Control
{
    private static readonly Color Background = new(0.055f, 0.065f, 0.075f, 0.94f);
    private static readonly Color Road = new(0.42f, 0.34f, 0.24f, 0.90f);
    private static readonly Color Field = new(0.34f, 0.31f, 0.16f, 0.80f);
    private static readonly Color Building = new(0.42f, 0.45f, 0.48f, 0.92f);
    private static readonly Color Home = new(0.53f, 0.45f, 0.33f, 0.95f);
    private static readonly Color Route = new(0.72f, 0.76f, 0.82f, 0.55f);
    private static readonly Color Player = new(0.95f, 0.95f, 0.95f, 1.0f);

    private VillageDebugSnapshot? _snapshot;
    private Rect2 _mapRect;

    internal void SetSnapshot(VillageDebugSnapshot? snapshot)
    {
        _snapshot = snapshot;
        QueueRedraw();
    }

    public override void _Draw()
    {
        _mapRect = new Rect2(8.0f, 8.0f, Math.Max(1.0f, Size.X - 16.0f), Math.Max(1.0f, Size.Y - 16.0f));
        DrawRect(_mapRect, Background);
        DrawFields();
        DrawRoads();
        DrawBuildings();

        if (_snapshot is null)
        {
            return;
        }

        foreach (var resident in _snapshot.Residents)
        {
            DrawResident(resident);
        }

        DrawPlayer(_snapshot.PlayerPosition);
    }

    private void DrawFields()
    {
        DrawWorldRect(new Vector3(-91.0f, 0.0f, 54.0f), new Vector2(42.0f, 58.0f), Field);
        DrawWorldRect(new Vector3(86.0f, 0.0f, 45.0f), new Vector2(46.0f, 64.0f), Field);
    }

    private void DrawRoads()
    {
        DrawRoad(new Vector3(0.0f, 0.0f, -102.5f), new Vector3(0.0f, 0.0f, 102.5f), VillageLayout.MainRoadWidthMeters);
        DrawRoad(new Vector3(-86.0f, 0.0f, 27.0f), new Vector3(0.0f, 0.0f, 27.0f), VillageLayout.SideRoadWidthMeters);
        DrawRoad(new Vector3(0.0f, 0.0f, -30.0f), new Vector3(84.0f, 0.0f, -30.0f), VillageLayout.SideRoadWidthMeters);
        DrawRoad(new Vector3(0.0f, 0.0f, -79.0f), new Vector3(36.0f, 0.0f, -79.0f), VillageLayout.SideRoadWidthMeters);
    }

    private void DrawBuildings()
    {
        foreach (var placement in VillageLayout.Buildings)
        {
            var rotated = Math.Abs((int)placement.YawDegrees) % 180 == 90;
            var footprint = rotated
                ? new Vector2(placement.Footprint.Y, placement.Footprint.X)
                : placement.Footprint;
            DrawWorldRect(
                placement.Position,
                footprint,
                placement.Kind == VillageBuildingKind.Home ? Home : Building);
        }
    }

    private void DrawResident(VillageDebugResidentSnapshot resident)
    {
        var from = WorldToMap(resident.Position);
        var previous = from;
        foreach (var waypoint in resident.Route)
        {
            var next = WorldToMap(waypoint);
            DrawLine(previous, next, Route, 1.4f, antialiased: true);
            previous = next;
        }

        if (resident.Route.Count == 0 && resident.DistanceToDestination > 0.25f)
        {
            DrawLine(from, WorldToMap(resident.Destination), Route, 1.2f, antialiased: true);
        }

        var destination = WorldToMap(resident.Destination);
        DrawLine(destination - new Vector2(3.0f, 0.0f), destination + new Vector2(3.0f, 0.0f), Route, 1.0f);
        DrawLine(destination - new Vector2(0.0f, 3.0f), destination + new Vector2(0.0f, 3.0f), Route, 1.0f);
        DrawCircle(from, 4.5f, ActivityColor(resident.Activity));
        if (!resident.RouteMatchesActivity)
        {
            DrawCircle(from, 7.0f, new Color(1.0f, 0.15f, 0.15f, 1.0f), filled: false, width: 1.5f);
        }
    }

    private void DrawPlayer(Vector3 position)
    {
        var point = WorldToMap(position);
        DrawCircle(point, 5.0f, Player, filled: false, width: 2.0f);
        DrawLine(point - new Vector2(5.0f, 0.0f), point + new Vector2(5.0f, 0.0f), Player, 1.5f);
        DrawLine(point - new Vector2(0.0f, 5.0f), point + new Vector2(0.0f, 5.0f), Player, 1.5f);
    }

    private void DrawRoad(Vector3 start, Vector3 end, float widthMeters)
    {
        var width = Math.Max(2.0f, widthMeters / VillageLayout.WidthMeters * _mapRect.Size.X);
        DrawLine(WorldToMap(start), WorldToMap(end), Road, width);
    }

    private void DrawWorldRect(Vector3 center, Vector2 footprint, Color color)
    {
        var centerPoint = WorldToMap(center);
        var size = new Vector2(
            footprint.X / VillageLayout.WidthMeters * _mapRect.Size.X,
            footprint.Y / VillageLayout.DepthMeters * _mapRect.Size.Y);
        DrawRect(new Rect2(centerPoint - (size * 0.5f), size), color);
    }

    private Vector2 WorldToMap(Vector3 position)
    {
        var normalizedX = (position.X + (VillageLayout.WidthMeters * 0.5f)) / VillageLayout.WidthMeters;
        var normalizedY = ((VillageLayout.DepthMeters * 0.5f) - position.Z) / VillageLayout.DepthMeters;
        return _mapRect.Position + new Vector2(normalizedX * _mapRect.Size.X, normalizedY * _mapRect.Size.Y);
    }

    private static Color ActivityColor(ResidentActivity activity) => activity switch
    {
        ResidentActivity.Working => new Color(0.95f, 0.67f, 0.20f),
        ResidentActivity.Eating => new Color(0.86f, 0.32f, 0.24f),
        ResidentActivity.Resting => new Color(0.62f, 0.43f, 0.88f),
        _ => new Color(0.25f, 0.70f, 0.80f),
    };
}
