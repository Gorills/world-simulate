using Godot;

namespace Mws.Client.Godot.World.Village;

internal static class VillageGeometryBuilder
{
    private const float WallThickness = 0.28f;
    private static readonly Color Grass = new(0.29f, 0.42f, 0.22f);
    private static readonly Color Road = new(0.37f, 0.28f, 0.18f);
    private static readonly Color Field = new(0.46f, 0.38f, 0.17f);
    private static readonly Color HomeWall = new(0.58f, 0.50f, 0.38f);
    private static readonly Color InnWall = new(0.49f, 0.38f, 0.28f);
    private static readonly Color WorkWall = new(0.42f, 0.43f, 0.38f);
    private static readonly Color StoreWall = new(0.47f, 0.42f, 0.31f);
    private static readonly Color BarnWall = new(0.38f, 0.27f, 0.20f);
    private static readonly Color Roof = new(0.20f, 0.16f, 0.14f);
    private static readonly Color InteriorFloor = new(0.30f, 0.24f, 0.18f);
    private static readonly Color Stone = new(0.42f, 0.44f, 0.42f);

    internal static void Build(Node3D root)
    {
        ArgumentNullException.ThrowIfNull(root);
        VillageLayout.Validate();

        AddBox(
            root,
            "VillageGround",
            new Vector3(0.0f, -0.25f, 0.0f),
            new Vector3(VillageLayout.WidthMeters, 0.5f, VillageLayout.DepthMeters),
            Grass,
            collision: true);

        AddRoad(root, "MainRoad", new Vector3(0.0f, 0.02f, 0.0f), new Vector2(VillageLayout.MainRoadWidthMeters, 205.0f));
        AddRoad(root, "NorthLane", new Vector3(-43.0f, 0.025f, 27.0f), new Vector2(86.0f, VillageLayout.SideRoadWidthMeters));
        AddRoad(root, "SouthLane", new Vector3(42.0f, 0.025f, -30.0f), new Vector2(84.0f, VillageLayout.SideRoadWidthMeters));
        AddRoad(root, "InnLane", new Vector3(18.0f, 0.025f, -79.0f), new Vector2(36.0f, VillageLayout.SideRoadWidthMeters));

        AddBox(root, "NorthField", new Vector3(-91.0f, 0.015f, 54.0f), new Vector3(42.0f, 0.03f, 58.0f), Field, collision: false);
        AddBox(root, "SouthField", new Vector3(86.0f, 0.015f, 45.0f), new Vector3(46.0f, 0.03f, 64.0f), Field, collision: false);
        AddBox(root, "VillageSquare", new Vector3(8.0f, 0.03f, 27.0f), new Vector3(22.0f, 0.04f, 18.0f), Road, collision: false);

        foreach (var placement in VillageLayout.Buildings)
        {
            BuildBuilding(root, placement);
        }

        BuildWell(root);
    }

    private static void AddRoad(Node3D root, string name, Vector3 position, Vector2 footprint) =>
        AddBox(root, name, position, new Vector3(footprint.X, 0.04f, footprint.Y), Road, collision: false);

    private static void BuildBuilding(Node3D root, VillageBuildingPlacement placement)
    {
        var building = new Node3D
        {
            Name = placement.Name,
            Position = placement.Position,
            RotationDegrees = new Vector3(0.0f, placement.YawDegrees, 0.0f),
        };
        root.AddChild(building);

        var width = placement.Footprint.X;
        var depth = placement.Footprint.Y;
        var wallHeight = placement.Kind switch
        {
            VillageBuildingKind.Inn => 3.7f,
            VillageBuildingKind.Barn => 4.1f,
            _ => 3.25f,
        };
        var wallColor = WallColor(placement.Kind);

        AddBox(
            building,
            "Floor",
            new Vector3(0.0f, 0.04f, 0.0f),
            new Vector3(width - 0.2f, 0.08f, depth - 0.2f),
            InteriorFloor,
            collision: false);
        AddBox(
            building,
            "BackWall",
            new Vector3(0.0f, wallHeight * 0.5f, -depth * 0.5f),
            new Vector3(width, wallHeight, WallThickness),
            wallColor,
            collision: true);
        AddBox(
            building,
            "LeftWall",
            new Vector3(-width * 0.5f, wallHeight * 0.5f, 0.0f),
            new Vector3(WallThickness, wallHeight, depth),
            wallColor,
            collision: true);
        AddBox(
            building,
            "RightWall",
            new Vector3(width * 0.5f, wallHeight * 0.5f, 0.0f),
            new Vector3(WallThickness, wallHeight, depth),
            wallColor,
            collision: true);

        var frontSegmentWidth = (width - placement.DoorWidth) * 0.5f;
        var frontOffset = (placement.DoorWidth + frontSegmentWidth) * 0.5f;
        AddBox(
            building,
            "FrontWallLeft",
            new Vector3(-frontOffset, wallHeight * 0.5f, depth * 0.5f),
            new Vector3(frontSegmentWidth, wallHeight, WallThickness),
            wallColor,
            collision: true);
        AddBox(
            building,
            "FrontWallRight",
            new Vector3(frontOffset, wallHeight * 0.5f, depth * 0.5f),
            new Vector3(frontSegmentWidth, wallHeight, WallThickness),
            wallColor,
            collision: true);

        AddBox(
            building,
            "Roof",
            new Vector3(0.0f, wallHeight + 0.16f, 0.0f),
            new Vector3(width + 0.7f, 0.32f, depth + 0.7f),
            Roof,
            collision: false);
        AddBox(
            building,
            "Threshold",
            new Vector3(0.0f, 0.055f, depth * 0.5f + 0.5f),
            new Vector3(placement.DoorWidth + 0.5f, 0.1f, 1.0f),
            InteriorFloor,
            collision: false);
        AddBox(
            building,
            "BuildingMarker",
            new Vector3(width * 0.5f - 0.5f, 2.15f, depth * 0.5f + 0.16f),
            new Vector3(0.55f, 0.55f, 0.12f),
            MarkerColor(placement.Kind),
            collision: false);
    }

    private static void BuildWell(Node3D root)
    {
        var well = new Node3D
        {
            Name = "VillageWell",
            Position = new Vector3(8.0f, 0.0f, 27.0f),
        };
        root.AddChild(well);

        AddBox(well, "NorthStone", new Vector3(0.0f, 0.45f, -1.0f), new Vector3(2.6f, 0.9f, 0.35f), Stone, collision: true);
        AddBox(well, "SouthStone", new Vector3(0.0f, 0.45f, 1.0f), new Vector3(2.6f, 0.9f, 0.35f), Stone, collision: true);
        AddBox(well, "WestStone", new Vector3(-1.0f, 0.45f, 0.0f), new Vector3(0.35f, 0.9f, 1.7f), Stone, collision: true);
        AddBox(well, "EastStone", new Vector3(1.0f, 0.45f, 0.0f), new Vector3(0.35f, 0.9f, 1.7f), Stone, collision: true);
    }

    private static void AddBox(
        Node3D parent,
        string name,
        Vector3 position,
        Vector3 size,
        Color color,
        bool collision)
    {
        var mesh = new MeshInstance3D
        {
            Name = $"{name}Mesh",
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.92f,
            },
        };

        if (!collision)
        {
            mesh.Position = position;
            parent.AddChild(mesh);
            return;
        }

        var body = new StaticBody3D
        {
            Name = name,
            Position = position,
        };
        body.AddChild(mesh);
        body.AddChild(new CollisionShape3D
        {
            Name = "Collision",
            Shape = new BoxShape3D { Size = size },
        });
        parent.AddChild(body);
    }

    private static Color WallColor(VillageBuildingKind kind) => kind switch
    {
        VillageBuildingKind.Home => HomeWall,
        VillageBuildingKind.Inn => InnWall,
        VillageBuildingKind.Workshop => WorkWall,
        VillageBuildingKind.Storehouse => StoreWall,
        VillageBuildingKind.Barn => BarnWall,
        _ => HomeWall,
    };

    private static Color MarkerColor(VillageBuildingKind kind) => kind switch
    {
        VillageBuildingKind.Home => new Color(0.74f, 0.66f, 0.47f),
        VillageBuildingKind.Inn => new Color(0.72f, 0.34f, 0.20f),
        VillageBuildingKind.Workshop => new Color(0.32f, 0.47f, 0.58f),
        VillageBuildingKind.Storehouse => new Color(0.72f, 0.57f, 0.20f),
        VillageBuildingKind.Barn => new Color(0.48f, 0.24f, 0.18f),
        _ => Colors.White,
    };
}
