using Godot;

namespace Mws.Client.Godot.World.Village;

internal static class VillageRoutePlanner
{
    private const float DuplicatePointDistance = 0.35f;

    private static readonly Vector3[] Nodes =
    [
        new(0.0f, 0.0f, 98.0f),
        new(0.0f, 0.0f, 72.0f),
        new(0.0f, 0.0f, 69.0f),
        new(0.0f, 0.0f, 54.0f),
        new(0.0f, 0.0f, 43.0f),
        new(0.0f, 0.0f, 40.0f),
        new(0.0f, 0.0f, 27.0f),
        new(0.0f, 0.0f, 13.0f),
        new(0.0f, 0.0f, 10.0f),
        new(0.0f, 0.0f, 0.0f),
        new(0.0f, 0.0f, -19.0f),
        new(0.0f, 0.0f, -22.0f),
        new(0.0f, 0.0f, -30.0f),
        new(0.0f, 0.0f, -50.0f),
        new(0.0f, 0.0f, -53.0f),
        new(0.0f, 0.0f, -70.0f),
        new(0.0f, 0.0f, -82.0f),
        new(0.0f, 0.0f, -98.0f),
        new(-43.0f, 0.0f, 27.0f),
        new(-86.0f, 0.0f, 27.0f),
        new(-91.0f, 0.0f, 54.0f),
        new(42.0f, 0.0f, -30.0f),
        new(84.0f, 0.0f, -30.0f),
        new(46.0f, 0.0f, 98.0f),
        new(92.0f, 0.0f, 98.0f),
        new(92.0f, 0.0f, 92.0f),
    ];

    private static readonly RouteEdge[] Edges =
    [
        new(0, 1),
        new(1, 2),
        new(2, 3),
        new(3, 4),
        new(4, 5),
        new(5, 6),
        new(6, 7),
        new(7, 8),
        new(8, 9),
        new(9, 10),
        new(10, 11),
        new(11, 12),
        new(12, 13),
        new(13, 14),
        new(14, 15),
        new(15, 16),
        new(16, 17),
        new(6, 18),
        new(18, 19),
        new(19, 20),
        new(12, 21),
        new(21, 22),
        new(0, 23),
        new(23, 24),
        new(24, 25),
    ];

    internal static IReadOnlyList<Vector3> Plan(
        Vector3 start,
        VillageResidentDestination destination)
    {
        var accessPoint = destination.AccessPoint ?? destination.Position;
        var startNode = FindNearestNode(start);
        var endNode = FindNearestNode(accessPoint);
        var nodePath = FindShortestNodePath(startNode, endNode);
        var result = new List<Vector3>(nodePath.Count + 2);

        foreach (var nodeIndex in nodePath)
        {
            AddIfDistinct(result, Nodes[nodeIndex], start);
        }

        AddIfDistinct(result, accessPoint, start);
        if (destination.AccessPoint.HasValue)
        {
            AddIfDistinct(result, destination.Position, start);
        }

        if (result.Count == 0)
        {
            result.Add(destination.Position);
        }

        return result;
    }

    internal static void Validate()
    {
        if (Edges.Any(edge => edge.From < 0 || edge.To < 0 || edge.From >= Nodes.Length || edge.To >= Nodes.Length))
        {
            throw new InvalidOperationException("Village route graph contains an invalid node reference.");
        }

        var samples = new[]
        {
            new VillageResidentDestination(VillageResidentDestinationKind.Work, VillageLayout.FarmWorkAnchor, null),
            new VillageResidentDestination(VillageResidentDestinationKind.Work, VillageLayout.HerbGroveWorkAnchor, null),
            ForBuilding(VillageResidentDestinationKind.Work, VillageLayout.CookWorkBuildingName),
            ForBuilding(VillageResidentDestinationKind.Food, VillageLayout.FoodBuildingName),
        };

        foreach (var sample in samples)
        {
            var route = Plan(VillageLayout.PlayerSpawn, sample);
            if (route.Count == 0 || route[^1].DistanceTo(sample.Position) > 0.01f)
            {
                throw new InvalidOperationException("Village route graph cannot reach a required life-simulation destination.");
            }
        }
    }

    private static int FindNearestNode(Vector3 position)
    {
        var bestIndex = 0;
        var bestDistance = float.PositiveInfinity;
        for (var index = 0; index < Nodes.Length; index++)
        {
            var distance = FlatDistanceSquared(position, Nodes[index]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = index;
            }
        }

        return bestIndex;
    }

    private static List<int> FindShortestNodePath(int start, int end)
    {
        var distances = new float[Nodes.Length];
        var previous = new int[Nodes.Length];
        var visited = new bool[Nodes.Length];
        Array.Fill(distances, float.PositiveInfinity);
        Array.Fill(previous, -1);
        distances[start] = 0.0f;

        for (var iteration = 0; iteration < Nodes.Length; iteration++)
        {
            var current = FindClosestUnvisited(distances, visited);
            if (current < 0 || current == end)
            {
                break;
            }

            visited[current] = true;
            foreach (var edge in Edges)
            {
                var neighbor = edge.Other(current);
                if (neighbor < 0 || visited[neighbor])
                {
                    continue;
                }

                var candidate = distances[current] + Nodes[current].DistanceTo(Nodes[neighbor]);
                if (candidate < distances[neighbor])
                {
                    distances[neighbor] = candidate;
                    previous[neighbor] = current;
                }
            }
        }

        if (start != end && previous[end] < 0)
        {
            throw new InvalidOperationException("Village route graph is disconnected.");
        }

        var path = new List<int>();
        for (var current = end; current >= 0; current = previous[current])
        {
            path.Add(current);
            if (current == start)
            {
                break;
            }
        }

        path.Reverse();
        return path;
    }

    private static int FindClosestUnvisited(float[] distances, bool[] visited)
    {
        var result = -1;
        var best = float.PositiveInfinity;
        for (var index = 0; index < distances.Length; index++)
        {
            if (!visited[index] && distances[index] < best)
            {
                best = distances[index];
                result = index;
            }
        }

        return result;
    }

    private static void AddIfDistinct(List<Vector3> result, Vector3 point, Vector3 start)
    {
        var previous = result.Count == 0 ? start : result[^1];
        if (previous.DistanceTo(point) > DuplicatePointDistance)
        {
            result.Add(point);
        }
    }

    private static float FlatDistanceSquared(Vector3 left, Vector3 right)
    {
        var x = left.X - right.X;
        var z = left.Z - right.Z;
        return (x * x) + (z * z);
    }

    private static VillageResidentDestination ForBuilding(
        VillageResidentDestinationKind kind,
        string buildingName)
    {
        var building = VillageLayout.GetBuilding(buildingName);
        return new VillageResidentDestination(
            kind,
            building.Position,
            VillageLayout.GetEntranceWorldPosition(building));
    }

    private readonly record struct RouteEdge(int From, int To)
    {
        internal int Other(int node) => node == From ? To : node == To ? From : -1;
    }
}
