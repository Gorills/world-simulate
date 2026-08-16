using Mws.Domain;

namespace Mws.Simulation.Api;

public static class ProofAVersions
{
    public const int CurrentSchemaVersion = 2;
    public const string CurrentModelVersion = "proof-a-model-v1";
    public const string CurrentConfigurationVersion = "proof-a-config-v1";
    public const string LegacyConfigurationVersion = "proof-a-config-v0";
    public const string LossyLegacyConfigurationVersion = "proof-a-config-lossy-v0";
}

public enum SnapshotCompatibility
{
    CompatibleDecode,
    DeterministicMigration,
    LossyMigrationRequired,
    Unsupported,
}

public sealed record ProofAEntityState(EntityId Id, EntityId OwnerId, long Resource, bool Rare);

public sealed record ProofATombstone(EntityId Id, SimulationTime DestroyedAt, string Reason);

public sealed record ProofAPendingProcess(
    long ProcessId,
    EntityId OwnerId,
    EntityId SubjectId,
    SimulationTime StartedAt,
    SimulationTime DueAt,
    long ReservedResource,
    bool Interruptible,
    long? StartTraceId);

public sealed record ProofABoundRandomOutcome(string Key, ulong Value);

public sealed record ProofACommandExecution(CommandId CommandId, bool Success, string Code, long? TraceId);

public sealed record ProofACausalTraceEntry(
    long TraceId,
    long? ParentTraceId,
    SimulationTime Time,
    string Kind,
    string Detail);

public sealed record ProofAKernelState(
    int SchemaVersion,
    string ModelVersion,
    string ConfigurationVersion,
    ulong WorldSeed,
    SimulationTime Time,
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

public sealed record ProofATransferIntent(
    CommandId CommandId,
    SimulationTime DueAt,
    int Priority,
    EntityId OwnerId,
    EntityId FromId,
    EntityId ToId,
    long Amount);

public sealed record ProofATransferResolution(CommandId CommandId, bool Success, string Code);

public sealed record ProofALodMember(EntityId Id, EntityId OwnerId, long Resource, bool Rare);

public sealed record ProofARegionAggregate(
    IReadOnlyList<ProofALodMember> Members,
    long TotalResource,
    IReadOnlyList<EntityId> RareEntityIds,
    IReadOnlyList<long> PendingProcessIds);
