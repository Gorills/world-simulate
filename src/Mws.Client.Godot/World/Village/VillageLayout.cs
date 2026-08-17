using Godot;

namespace Mws.Client.Godot.World.Village;

internal enum VillageBuildingKind
{
    Home,
    Inn,
    Workshop,
    Storehouse,
    Barn,
}

internal readonly record struct VillageBuildingPlacement(
    string Name,
    VillageBuildingKind Kind,
    Vector3 Position,
    Vector2 Footprint,
    float YawDegrees,
    float DoorWidth);

internal static class VillageLayout
{
    internal const float WidthMeters = 260.0f;
    internal const float DepthMeters = 220.0f;
    internal const float MainRoadWidthMeters = 7.0f;
    internal const float SideRoadWidthMeters = 5.0f;
    internal const float MinimumBuildingCenterSpacingMeters = 14.0f;

    internal static readonly Vector3 PlayerSpawn = new(3.0f, 0.2f, 94.0f);
    internal static readonly Vector3 StockpileOrigin = new(54.0f, 0.15f, -22.0f);

    internal static readonly Vector3[] ResidentSpawns =
    [
        new(-12.0f, 0.0f, 54.0f),
        new(10.0f, 0.0f, 12.0f),
        new(-10.0f, 0.0f, -42.0f),
        new(18.0f, 0.0f, -70.0f),
        new(-42.0f, 0.0f, 24.0f),
        new(44.0f, 0.0f, -30.0f),
    ];

    internal static readonly VillageBuildingPlacement[] Buildings =
    [
        Home("North House West", -19.0f, 72.0f, 90.0f),
        Home("North House East", 19.0f, 69.0f, -90.0f),
        Home("Miller House", -20.0f, 43.0f, 90.0f),
        Home("Cook House", 20.0f, 40.0f, -90.0f),
        Home("River House", -19.0f, 13.0f, 90.0f),
        Home("Grove House", 19.0f, 10.0f, -90.0f),
        Home("South House West", -20.0f, -19.0f, 90.0f),
        Home("South House East", 20.0f, -22.0f, -90.0f),
        Home("Far South House West", -19.0f, -50.0f, 90.0f),
        Home("Far South House East", 19.0f, -53.0f, -90.0f),
        new VillageBuildingPlacement(
            "The Hearth Inn",
            VillageBuildingKind.Inn,
            new Vector3(24.0f, 0.0f, -82.0f),
            new Vector2(12.0f, 16.0f),
            -90.0f,
            2.1f),
        new VillageBuildingPlacement(
            "Carpenter Workshop",
            VillageBuildingKind.Workshop,
            new Vector3(-52.0f, 0.0f, 27.0f),
            new Vector2(11.0f, 13.0f),
            0.0f,
            2.0f),
        new VillageBuildingPlacement(
            "Common Storehouse",
            VillageBuildingKind.Storehouse,
            new Vector3(55.0f, 0.0f, -29.0f),
            new Vector2(12.0f, 14.0f),
            180.0f,
            2.2f),
        new VillageBuildingPlacement(
            "South Barn",
            VillageBuildingKind.Barn,
            new Vector3(-68.0f, 0.0f, -65.0f),
            new Vector2(15.0f, 20.0f),
            0.0f,
            3.2f),
    ];

    internal static void Validate()
    {
        if (WidthMeters < 200.0f || DepthMeters < 180.0f)
        {
            throw new InvalidOperationException("Village playable footprint is too compact for the spatial target.");
        }

        if (MainRoadWidthMeters < 6.0f || SideRoadWidthMeters < 4.0f)
        {
            throw new InvalidOperationException("Village roads are narrower than the playable spatial contract.");
        }

        if (Buildings.Length < 12)
        {
            throw new InvalidOperationException("Village greybox must contain a meaningful settlement footprint.");
        }

        for (var index = 0; index < Buildings.Length; index++)
        {
            var building = Buildings[index];
            if (building.Footprint.X < 7.0f
                || building.Footprint.Y < 8.0f
                || building.DoorWidth < 1.4f)
            {
                throw new InvalidOperationException($"Building '{building.Name}' violates playable size or doorway bounds.");
            }

            for (var otherIndex = index + 1; otherIndex < Buildings.Length; otherIndex++)
            {
                var other = Buildings[otherIndex];
                var distance = new Vector2(
                    building.Position.X - other.Position.X,
                    building.Position.Z - other.Position.Z).Length();
                if (distance < MinimumBuildingCenterSpacingMeters)
                {
                    throw new InvalidOperationException(
                        $"Buildings '{building.Name}' and '{other.Name}' are packed too closely for the village target.");
                }
            }
        }
    }

    private static VillageBuildingPlacement Home(string name, float x, float z, float yawDegrees) =>
        new(
            name,
            VillageBuildingKind.Home,
            new Vector3(x, 0.0f, z),
            new Vector2(8.5f, 10.0f),
            yawDegrees,
            1.6f);
}
