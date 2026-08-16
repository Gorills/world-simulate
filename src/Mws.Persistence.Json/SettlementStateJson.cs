using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mws.Simulation.Api;

namespace Mws.Persistence.Json;

public static class SettlementStateJson
{
    public static string Serialize(SettlementState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.SchemaVersion != SettlementVersions.CurrentSchemaVersion)
        {
            throw new NotSupportedException($"Settlement schema {state.SchemaVersion} is unsupported.");
        }

        var payload = JsonSerializer.Serialize(state, SettlementStateJsonContext.Default.SettlementState);
        var envelope = new SettlementSnapshotEnvelope(state.SchemaVersion, payload, ComputeChecksum(payload));
        return JsonSerializer.Serialize(envelope, SettlementStateJsonContext.Default.SettlementSnapshotEnvelope);
    }

    public static SettlementState Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var envelope = JsonSerializer.Deserialize(json, SettlementStateJsonContext.Default.SettlementSnapshotEnvelope)
            ?? throw new InvalidDataException("Settlement snapshot envelope is missing.");
        if (!string.Equals(envelope.Checksum, ComputeChecksum(envelope.Payload), StringComparison.Ordinal))
        {
            throw new InvalidDataException("Settlement snapshot checksum mismatch.");
        }

        if (envelope.SchemaVersion != SettlementVersions.CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Settlement schema {envelope.SchemaVersion} is unsupported; expected {SettlementVersions.CurrentSchemaVersion}.");
        }

        var state = JsonSerializer.Deserialize(envelope.Payload, SettlementStateJsonContext.Default.SettlementState)
            ?? throw new InvalidDataException("Settlement snapshot payload is missing.");
        if (envelope.SchemaVersion != state.SchemaVersion)
        {
            throw new InvalidDataException("Settlement snapshot schema markers disagree.");
        }

        return state;
    }

    private static string ComputeChecksum(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
}

internal sealed record SettlementSnapshotEnvelope(int SchemaVersion, string Payload, string Checksum);

[JsonSerializable(typeof(SettlementState))]
[JsonSerializable(typeof(SettlementSnapshotEnvelope))]
internal sealed partial class SettlementStateJsonContext : JsonSerializerContext
{
}
