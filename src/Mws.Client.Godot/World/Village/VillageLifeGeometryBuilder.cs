using Godot;

namespace Mws.Client.Godot.World.Village;

internal static class VillageLifeGeometryBuilder
{
    private static readonly Color Track = new(0.33f, 0.25f, 0.16f);
    private static readonly Color GroveGround = new(0.24f, 0.36f, 0.18f);
    private static readonly Color Herb = new(0.18f, 0.49f, 0.21f);
    private static readonly Color WorkMarker = new(0.82f, 0.67f, 0.24f);

    internal static void Build(Node3D root)
    {
        ArgumentNullException.ThrowIfNull(root);

        AddPatch(root, "FarmTrackWest", new Vector3(-43.0f, 0.035f, 27.0f), new Vector3(86.0f, 0.03f, 3.0f), Track);
        AddPatch(root, "FarmTrackNorth", new Vector3(-89.0f, 0.035f, 40.5f), new Vector3(3.0f, 0.03f, 27.0f), Track);
        AddPatch(root, "GroveTrackEast", new Vector3(46.0f, 0.035f, 98.0f), new Vector3(92.0f, 0.03f, 3.0f), Track);
        AddPatch(root, "GroveTrackSouth", new Vector3(92.0f, 0.035f, 95.0f), new Vector3(3.0f, 0.03f, 6.0f), Track);
        AddPatch(root, "HerbGroveGround", new Vector3(92.0f, 0.025f, 92.0f), new Vector3(18.0f, 0.04f, 18.0f), GroveGround);

        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                AddPatch(
                    root,
                    $"Herb-{row}-{column}",
                    new Vector3(87.5f + (column * 3.0f), 0.32f, 88.5f + (row * 3.0f)),
                    new Vector3(0.45f, 0.64f, 0.45f),
                    Herb);
            }
        }

        AddPatch(root, "FarmWorkMarker", VillageLayout.FarmWorkAnchor + new Vector3(0.0f, 0.08f, 0.0f), new Vector3(2.2f, 0.16f, 2.2f), WorkMarker);
        AddPatch(root, "GroveWorkMarker", VillageLayout.HerbGroveWorkAnchor + new Vector3(0.0f, 0.08f, 0.0f), new Vector3(2.2f, 0.16f, 2.2f), WorkMarker);
    }

    private static void AddPatch(Node3D root, string name, Vector3 position, Vector3 size, Color color)
    {
        root.AddChild(new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.94f,
            },
        });
    }
}
