using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mws.Simulation.Api;

namespace Mws.Persistence.Json;

public sealed record ProofASnapshotLoadResult(ProofAKernelState State, SnapshotCompatibility Compatibility);

public static class ProofAKernelJson
{
    public static string Serialize(ProofAKernelState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var payload = JsonSerializer.Serialize(state, ProofAKernelJsonContext.Default.ProofAKernelState);
        var envelope = new ProofASnapshotEnvelope(
            ProofAVersions.CurrentSchemaVersion,
            payload,
            ComputeChecksum(payload));
        return JsonSerializer.Serialize(envelope, ProofAKernelJsonContext.Default.ProofASnapshotEnvelope);
    }

    public static string SerializeLegacyV1Fixture(ProofAKernelState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var legacy = new LegacyProofAKernelStateV1(
            state.ModelVersion,
            ProofAVersions.LegacyConfigurationVersion,
            state.WorldSeed,
            state.Time,
            state.NextEntityId,
            state.NextCommandId,
            state.NextProcessId,
            state.NextTraceId,
            state.Entities,
            state.Tombstones,
            state.PendingProcesses,
            state.BoundRandomOutcomes,
            state.CommandLedger,
            state.Trace);
        var payload = JsonSerializer.Serialize(legacy, ProofAKernelJsonContext.Default.LegacyProofAKernelStateV1);
        var envelope = new ProofASnapshotEnvelope(1, payload, ComputeChecksum(payload));
        return JsonSerializer.Serialize(envelope, ProofAKernelJsonContext.Default.ProofASnapshotEnvelope);
    }

    public static ProofASnapshotLoadResult Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var envelope = JsonSerializer.Deserialize(json, ProofAKernelJsonContext.Default.ProofASnapshotEnvelope)
            ?? throw new InvalidDataException("Snapshot envelope is missing.");
        ValidateChecksum(envelope);

        return envelope.SchemaVersion switch
        {
            ProofAVersions.CurrentSchemaVersion => LoadCurrent(envelope.Payload),
            1 => MigrateV1(envelope.Payload),
            _ => throw new NotSupportedException("Snapshot schema version is unsupported."),
        };
    }

    public static bool TryDeserialize(string json, out ProofASnapshotLoadResult? result, out string error)
    {
        try
        {
            result = Deserialize(json);
            error = string.Empty;
            return true;
        }
        catch (JsonException exception)
        {
            result = null;
            error = exception.Message;
            return false;
        }
        catch (InvalidDataException exception)
        {
            result = null;
            error = exception.Message;
            return false;
        }
        catch (NotSupportedException exception)
        {
            result = null;
            error = exception.Message;
            return false;
        }
        catch (ArgumentException exception)
        {
            result = null;
            error = exception.Message;
            return false;
        }
    }

    private static ProofASnapshotLoadResult LoadCurrent(string payload)
    {
        var state = JsonSerializer.Deserialize(payload, ProofAKernelJsonContext.Default.ProofAKernelState)
            ?? throw new InvalidDataException("Snapshot payload is missing.");
        if (state.SchemaVersion != ProofAVersions.CurrentSchemaVersion)
        {
            throw new InvalidDataException("Snapshot payload schema marker does not match its envelope.");
        }

        return new ProofASnapshotLoadResult(state, SnapshotCompatibility.CompatibleDecode);
    }

    private static ProofASnapshotLoadResult MigrateV1(string payload)
    {
        var legacy = JsonSerializer.Deserialize(payload, ProofAKernelJsonContext.Default.LegacyProofAKernelStateV1)
            ?? throw new InvalidDataException("Legacy snapshot payload is missing.");
        var migrated = new ProofAKernelState(
            ProofAVersions.CurrentSchemaVersion,
            legacy.ModelVersion,
            legacy.ConfigurationVersion,
            legacy.WorldSeed,
            legacy.Time,
            legacy.NextEntityId,
            legacy.NextCommandId,
            legacy.NextProcessId,
            legacy.NextTraceId,
            legacy.Entities,
            legacy.Tombstones,
            legacy.PendingProcesses,
            legacy.BoundRandomOutcomes,
            legacy.CommandLedger,
            legacy.Trace);
        return new ProofASnapshotLoadResult(migrated, SnapshotCompatibility.DeterministicMigration);
    }

    private static void ValidateChecksum(ProofASnapshotEnvelope envelope)
    {
        if (!string.Equals(envelope.Checksum, ComputeChecksum(envelope.Payload), StringComparison.Ordinal))
        {
            throw new InvalidDataException("Snapshot checksum mismatch.");
        }
    }

    private static string ComputeChecksum(string payload) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
}

internal sealed record ProofASnapshotEnvelope(int SchemaVersion, string Payload, string Checksum);

internal sealed record LegacyProofAKernelStateV1(
    string ModelVersion,
    string ConfigurationVersion,
    ulong WorldSeed,
    Mws.Domain.SimulationTime Time,
    long NextEntityId,
    long NextCommandId,
    long NextProcessId,
    long NextTraceId,
    IReadOnlyList<ProofAEntityState> Entities,
    IReadOnlyList<ProofATombstone> Tombstones,
    IReadOnlyList<ProofAPendingProcess> PendingProcesses,
    IReadOnlyList<ProofABoundRandomOutcome> BoundRandomOutcomes,
    IReadOnlyList<ProofACommandExecution> CommandLedger,
    IReadOnlyList<ProofACausalTraceEntry> Trace);

[JsonSerializable(typeof(ProofAKernelState))]
[JsonSerializable(typeof(ProofASnapshotEnvelope))]
[JsonSerializable(typeof(LegacyProofAKernelStateV1))]
internal sealed partial class ProofAKernelJsonContext : JsonSerializerContext
{
}
