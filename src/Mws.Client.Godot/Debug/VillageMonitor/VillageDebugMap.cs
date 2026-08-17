using Godot;
using Mws.Client.Godot.UI.Theme;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Debug.VillageMonitor;

public partial class VillageDebugMap : Control
{
    private VillageDebugSnapshot? _snapshot;
    private Rect2 _mapRect;

    internal void SetSnapshot(VillageDebugSnapshot? snapshot)
    {
        _snapshot = snapshot;
        QueueRedraw();
    }

    public override void _Draw()
    {
        _mapRect = new Rect2(
            8.0f,
            8.0f,
            Math.Max(1.0f, Size.X - 16.0f),
            Math.Max(1.0f, Size.Y - 16.0f));
        DrawRect(_mapRect, DesignSystem.DataColor(UiDataColor.MapBackground));
        DrawRect(
            _mapRect,
            DesignSystem.DataColor(UiDataColor.Building),
            filled: false,
            width: 1.0f);
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
        DrawWorldRect(
            new Vector3(-91.0f, 0.0f, 54.0f),
            new Vector2(42.0f, 58.0f),
            DesignSystem.DataColor(UiDataColor.Field));
        DrawWorldRect(
            new Vector3(86.0f, 0.0f, 45.0f),
            new Vector2(46.0f, 64.0f),
            DesignSystem.DataColor(UiDataColor.Field));
    }

    private void DrawRoads()
    {
        DrawRoad(
            new Vector3(0.0f, 0.0f, -102.5f),
            new Vector3(0.0f, 0.0f, 102.5f),
            VillageLayout.MainRoadWidthMeters);
        DrawRoad(
            new Vector3(-86.0f, 0.0f, 27.0f),
            new Vector3(0.0f, 0.0f, 27.0f),
            VillageLayout.SideRoadWidthMeters);
        DrawRoad(
            new Vector3(0.0f, 0.0f, -30.0f),
            new Vector3(84.0f, 0.0f, -30.0f),
            VillageLayout.SideRoadWidthMeters);
        DrawRoad(
            new Vector3(0.0f, 0.0f, -79.0f),
            new Vector3(36.0f, 0.0f, -79.0f),
            VillageLayout.SideRoadWidthMeters);
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
                DesignSystem.DataColor(
                    placement.Kind == VillageBuildingKind.Home
                        ? UiDataColor.Home
                        : UiDataColor.Building));
        }
    }

    private void DrawResident(VillageDebugResidentSnapshot resident)
    {
        var routeColor = DesignSystem.DataColor(UiDataColor.Route);
        var from = WorldToMap(resident.Position);
        var previous = from;
        foreach (var waypoint in resident.Route)
        {
            var next = WorldToMap(waypoint);
            DrawLine(previous, next, routeColor, 1.4f, antialiased: true);
            previous = next;
        }

        if (resident.Route.Count == 0 && resident.DistanceToDestination > 0.25f)
        {
            DrawLine(from, WorldToMap(resident.Destination), routeColor, 1.2f, antialiased: true);
        }

        var destination = WorldToMap(resident.Destination);
        DrawLine(
            destination - new Vector2(3.0f, 0.0f),
            destination + new Vector2(3.0f, 0.0f),
            routeColor,
            1.0f);
        DrawLine(
            destination - new Vector2(0.0f, 3.0f),
            destination + new Vector2(0.0f, 3.0f),
            routeColor,
            1.0f);
        DrawCircle(from, 4.5f, ActivityColor(resident.Activity));
        if (!resident.RouteMatchesActivity)
        {
            DrawCircle(
                from,
                7.0f,
                DesignSystem.DataColor(UiDataColor.Danger),
                filled: false,
                width: 1.5f);
        }
    }

    private void DrawPlayer(Vector3 position)
    {
        var playerColor = DesignSystem.DataColor(UiDataColor.Player);
        var point = WorldToMap(position);
        DrawCircle(point, 5.0f, playerColor, filled: false, width: 2.0f);
        DrawLine(
            point - new Vector2(5.0f, 0.0f),
            point + new Vector2(5.0f, 0.0f),
            playerColor,
            1.5f);
        DrawLine(
            point - new Vector2(0.0f, 5.0f),
            point + new Vector2(0.0f, 5.0f),
            playerColor,
            1.5f);
    }

    private void DrawRoad(Vector3 start, Vector3 end, float widthMeters)
    {
        var width = Math.Max(2.0f, widthMeters / VillageLayout.WidthMeters * _mapRect.Size.X);
        DrawLine(
            WorldToMap(start),
            WorldToMap(end),
            DesignSystem.DataColor(UiDataColor.Road),
            width);
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
        var normalizedX =
            (position.X + (VillageLayout.WidthMeters * 0.5f)) / VillageLayout.WidthMeters;
        var normalizedY =
            ((VillageLayout.DepthMeters * 0.5f) - position.Z) / VillageLayout.DepthMeters;
        return _mapRect.Position
            + new Vector2(normalizedX * _mapRect.Size.X, normalizedY * _mapRect.Size.Y);
    }

    private static Color ActivityColor(ResidentActivity activity) => activity switch
    {
        ResidentActivity.Working => DesignSystem.DataColor(UiDataColor.Working),
        ResidentActivity.Eating => DesignSystem.DataColor(UiDataColor.Eating),
        ResidentActivity.Resting => DesignSystem.DataColor(UiDataColor.Resting),
        _ => DesignSystem.DataColor(UiDataColor.Idle),
    };
}
