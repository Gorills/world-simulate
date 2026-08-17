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
        ValidateCurrentVersionBundle(state);

        var payload = JsonSerializer.Serialize(
            state,
            SettlementStateJsonContext.Default.SettlementState);
        var envelope = new SettlementSnapshotEnvelope(
            state.SchemaVersion,
            payload,
            ComputeChecksum(payload));
        return JsonSerializer.Serialize(
            envelope,
            SettlementStateJsonContext.Default.SettlementSnapshotEnvelope);
    }

    public static SettlementState Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var envelope = JsonSerializer.Deserialize(
            json,
            SettlementStateJsonContext.Default.SettlementSnapshotEnvelope)
            ?? throw new InvalidDataException("Settlement snapshot envelope is missing.");
        if (!string.Equals(
            envelope.Checksum,
            ComputeChecksum(envelope.Payload),
            StringComparison.Ordinal))
        {
            throw new InvalidDataException("Settlement snapshot checksum mismatch.");
        }

        return envelope.SchemaVersion switch
        {
            SettlementVersions.CurrentSchemaVersion => LoadCurrent(envelope.Payload),
            SettlementVersions.PreviousSchemaVersion => MigrateV4(envelope.Payload),
            SettlementVersions.LegacySchemaVersion => MigrateV3(envelope.Payload),
            _ => throw new NotSupportedException(
                $"Settlement schema {envelope.SchemaVersion} is unsupported; " +
                $"expected {SettlementVersions.CurrentSchemaVersion}."),
        };
    }

    private static SettlementState LoadCurrent(string payload)
    {
        var state = JsonSerializer.Deserialize(
            payload,
            SettlementStateJsonContext.Default.SettlementState)
            ?? throw new InvalidDataException("Settlement snapshot payload is missing.");
        ValidateCurrentVersionBundle(state);
        return state;
    }

    private static SettlementState MigrateV4(string payload)
    {
        var legacy = JsonSerializer.Deserialize(
            payload,
            SettlementStateJsonContext.Default.LegacySettlementStateV4)
            ?? throw new InvalidDataException("Legacy v4 settlement snapshot payload is missing.");
        if (legacy.SchemaVersion != SettlementVersions.PreviousSchemaVersion)
        {
            throw new InvalidDataException("Legacy v4 settlement snapshot schema markers disagree.");
        }

        if (!string.Equals(
                legacy.ModelVersion,
                SettlementVersions.CurrentModelVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                legacy.RulesVersion,
                SettlementVersions.CurrentRulesVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                legacy.ContentVersion,
                SettlementVersions.CurrentContentVersion,
                StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                "Legacy v4 settlement model, rules, or content version is unsupported.");
        }

        return CreateMigratedState(
            legacy.ScopeId,
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

    private static SettlementState MigrateV3(string payload)
    {
        var legacy = JsonSerializer.Deserialize(
            payload,
            SettlementStateJsonContext.Default.LegacySettlementStateV3)
            ?? throw new InvalidDataException("Legacy v3 settlement snapshot payload is missing.");
        if (legacy.SchemaVersion != SettlementVersions.LegacySchemaVersion)
        {
            throw new InvalidDataException("Legacy v3 settlement snapshot schema markers disagree.");
        }

        return CreateMigratedState(
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

    private static SettlementState CreateMigratedState(
        SimulationScopeId scopeId,
        ulong worldSeed,
        SimulationTime time,
        long nextEventId,
        long nextStackId,
        long nextCommandId,
        EntityId settlementOwnerId,
        IReadOnlyList<LegacyResidentState> residents,
        IReadOnlyList<ItemStackState> itemStacks,
        IReadOnlyList<WorkplaceState> workplaces,
        IReadOnlyList<SettlementEvent> events,
        IReadOnlyList<SettlementCommandReceipt> commandReceipts) =>
        new(
            SettlementVersions.CurrentSchemaVersion,
            SettlementVersions.CurrentModelVersion,
            SettlementVersions.CurrentRulesVersion,
            SettlementVersions.CurrentContentVersion,
            scopeId,
            worldSeed,
            time,
            nextEventId,
            nextStackId,
            nextCommandId,
            settlementOwnerId,
            residents.Select(MigrateResident).ToArray(),
            itemStacks,
            workplaces,
            events,
            commandReceipts,
            [],
            []);

    private static ResidentState MigrateResident(LegacyResidentState resident) =>
        new(
            resident.Id,
            resident.Name,
            resident.Hunger,
            resident.Energy,
            resident.Activity,
            resident.Profession,
            resident.WorkplaceId,
            resident.Affinity,
            default);

    private static void ValidateCurrentVersionBundle(SettlementState state)
    {
        if (state.SchemaVersion != SettlementVersions.CurrentSchemaVersion)
        {
            throw new NotSupportedException($"Settlement schema {state.SchemaVersion} is unsupported.");
        }

        if (!string.Equals(
                state.ModelVersion,
                SettlementVersions.CurrentModelVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                state.RulesVersion,
                SettlementVersions.CurrentRulesVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                state.ContentVersion,
                SettlementVersions.CurrentContentVersion,
                StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                "Settlement model, rules, or content version is unsupported.");
        }
    }

    private static string ComputeChecksum(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
}

internal sealed record SettlementSnapshotEnvelope(
    int SchemaVersion,
    string Payload,
    string Checksum);

internal sealed record LegacyResidentState(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity,
    ResidentProfession Profession,
    EntityId WorkplaceId,
    int Affinity);

internal sealed record LegacySettlementStateV4(
    int SchemaVersion,
    string ModelVersion,
    string RulesVersion,
    string ContentVersion,
    SimulationScopeId ScopeId,
    ulong WorldSeed,
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    long NextCommandId,
    EntityId SettlementOwnerId,
    IReadOnlyList<LegacyResidentState> Residents,
    IReadOnlyList<ItemStackState> ItemStacks,
    IReadOnlyList<WorkplaceState> Workplaces,
    IReadOnlyList<SettlementEvent> Events,
    IReadOnlyList<SettlementCommandReceipt> CommandReceipts);

internal sealed record LegacySettlementStateV3(
    int SchemaVersion,
    ulong WorldSeed,
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    long NextCommandId,
    EntityId SettlementOwnerId,
    IReadOnlyList<LegacyResidentState> Residents,
    IReadOnlyList<ItemStackState> ItemStacks,
    IReadOnlyList<WorkplaceState> Workplaces,
    IReadOnlyList<SettlementEvent> Events,
    IReadOnlyList<SettlementCommandReceipt> CommandReceipts);

[JsonSerializable(typeof(SettlementState))]
[JsonSerializable(typeof(SettlementSnapshotEnvelope))]
[JsonSerializable(typeof(LegacySettlementStateV4))]
[JsonSerializable(typeof(LegacySettlementStateV3))]
internal sealed partial class SettlementStateJsonContext : JsonSerializerContext
{
}
