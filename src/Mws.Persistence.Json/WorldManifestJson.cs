using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mws.Simulation.Api;

namespace Mws.Persistence.Json;

public static class WorldManifestJson
{
    public static string Serialize(WorldManifestState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateVersion(state);
        var payload = JsonSerializer.Serialize(state, WorldManifestJsonContext.Default.WorldManifestState);
        var envelope = new WorldManifestEnvelope(state.SchemaVersion, payload, ComputeChecksum(payload));
        return JsonSerializer.Serialize(envelope, WorldManifestJsonContext.Default.WorldManifestEnvelope);
    }

    public static WorldManifestState Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var envelope = JsonSerializer.Deserialize(json, WorldManifestJsonContext.Default.WorldManifestEnvelope)
            ?? throw new InvalidDataException("World manifest envelope is missing.");
        if (!string.Equals(envelope.Checksum, ComputeChecksum(envelope.Payload), StringComparison.Ordinal))
        {
            throw new InvalidDataException("World manifest checksum mismatch.");
        }

        if (envelope.SchemaVersion != WorldVersions.CurrentSchemaVersion)
        {
            throw new NotSupportedException($"World schema {envelope.SchemaVersion} is unsupported.");
        }

        var state = JsonSerializer.Deserialize(envelope.Payload, WorldManifestJsonContext.Default.WorldManifestState)
            ?? throw new InvalidDataException("World manifest payload is missing.");
        ValidateVersion(state);
        return state;
    }

    private static void ValidateVersion(WorldManifestState state)
    {
        if (state.SchemaVersion != WorldVersions.CurrentSchemaVersion
            || !string.Equals(state.ModelVersion, WorldVersions.CurrentModelVersion, StringComparison.Ordinal)
            || !string.Equals(state.RulesVersion, WorldVersions.CurrentRulesVersion, StringComparison.Ordinal)
            || !string.Equals(state.ContentVersion, WorldVersions.CurrentContentVersion, StringComparison.Ordinal)
            || !WorldSystemVersions.IsCurrent(state.SystemVersions))
        {
            throw new NotSupportedException("World manifest version bundle is unsupported.");
        }
    }

    private static string ComputeChecksum(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
}

internal sealed record WorldManifestEnvelope(int SchemaVersion, string Payload, string Checksum);

[JsonSerializable(typeof(WorldManifestState))]
[JsonSerializable(typeof(WorldManifestEnvelope))]
internal sealed partial class WorldManifestJsonContext : JsonSerializerContext
{
}
