using System.Text.Json;
using System.Text.Json.Serialization;
using Mws.Simulation.Api;

namespace Mws.Persistence.Json;

public static class WorldSnapshotJson
{
    public static string Serialize(WorldSnapshot snapshot) =>
        JsonSerializer.Serialize(snapshot, WorldSnapshotJsonContext.Default.WorldSnapshot);

    public static WorldSnapshot Deserialize(string json) =>
        JsonSerializer.Deserialize(json, WorldSnapshotJsonContext.Default.WorldSnapshot);
}

[JsonSerializable(typeof(WorldSnapshot))]
internal sealed partial class WorldSnapshotJsonContext : JsonSerializerContext
{
}
