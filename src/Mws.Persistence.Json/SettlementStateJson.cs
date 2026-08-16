using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mws.Domain;
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

        return envelope.SchemaVersion switch
        {
            SettlementVersions.CurrentSchemaVersion => LoadCurrent(envelope.Payload),
            SettlementVersions.LegacySchemaVersion => MigrateV3(envelope.Payload),
            _ => throw new NotSupportedException(
                $"Settlement schema {envelope.SchemaVersion} is unsupported; expected {SettlementVersions.CurrentSchemaVersion}."),
        };
    }

    private static SettlementState LoadCurrent(string payload)
    {
        var state = JsonSerializer.Deserialize(payload, SettlementStateJsonContext.Default.SettlementState)
            ?? throw new InvalidDataException("Settlement snapshot payload is missing.");
        if (state.SchemaVersion != SettlementVersions.CurrentSchemaVersion)
        {
            throw new InvalidDataException("Settlement snapshot schema markers disagree.");
        }

        return state;
    }

    private static SettlementState MigrateV3(string payload)
    {
        var legacy = JsonSerializer.Deserialize(payload, SettlementStateJsonContext.Default.LegacySettlementStateV3)
            ?? throw new InvalidDataException("Legacy settlement snapshot payload is missing.");
        if (legacy.SchemaVersion != SettlementVersions.LegacySchemaVersion)
        {
            throw new InvalidDataException("Legacy settlement snapshot schema markers disagree.");
        }

        return new SettlementState(
            SettlementVersions.CurrentSchemaVersion,
            SettlementVersions.CurrentModelVersion,
            SettlementVersions.CurrentRulesVersion,
            SettlementVersions.CurrentContentVersion,
            SimulationScopeId.Root,
            legacy.WorldSeed,
            legacy.Time,
            legacy.NextEventId,
            legacy.NextStackId,
            legacy.NextCommandId,
            legacy.SettlementOwnerId,
            legacy.Residents,
            legacy.ItemStacks,
            legacy.Workplaces,
            legacy.Events,
            legacy.CommandReceipts);
    }

    private static string ComputeChecksum(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
}

internal sealed record SettlementSnapshotEnvelope(int SchemaVersion, string Payload, string Checksum);

internal sealed record LegacySettlementStateV3(
    int SchemaVersion,
    ulong WorldSeed,
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    long NextCommandId,
    EntityId SettlementOwnerId,
    IReadOnlyList<ResidentState> Residents,
    IReadOnlyList<ItemStackState> ItemStacks,
    IReadOnlyList<WorkplaceState> Workplaces,
    IReadOnlyList<SettlementEvent> Events,
    IReadOnlyList<SettlementCommandReceipt> CommandReceipts);

[JsonSerializable(typeof(SettlementState))]
[JsonSerializable(typeof(SettlementSnapshotEnvelope))]
[JsonSerializable(typeof(LegacySettlementStateV3))]
internal sealed partial class SettlementStateJsonContext : JsonSerializerContext
{
}
