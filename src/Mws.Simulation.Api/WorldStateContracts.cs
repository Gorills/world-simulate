using Mws.Domain;

namespace Mws.Simulation.Api;

public static class WorldVersions
{
    public const int CurrentSchemaVersion = 1;
    public const string CurrentModelVersion = "world-model-v1";
    public const string CurrentRulesVersion = "world-rules-v1";
    public const string CurrentContentVersion = "world-content-v1";
}

public static class WorldPartitionKinds
{
    public const string Settlement = "settlement";
}

public static class WorldOperationKinds
{
    public const string ResidentMigration = "resident-migration";
}

public sealed record WorldPartitionDescriptor(
    SimulationScopeId ScopeId,
    string Kind,
    long Revision);

public sealed record WorldEntityLocation(
    EntityId EntityId,
    SimulationScopeId ScopeId);

public sealed record ResidentMigrationIntent(
    WorldOperationId OperationId,
    EntityId ResidentId,
    SimulationScopeId SourceScopeId,
    SimulationScopeId DestinationScopeId);

public sealed record WorldOperationReceipt(
    WorldOperationId OperationId,
    string Kind,
    bool Success,
    string Code,
    EntityId? SubjectId,
    SimulationScopeId? SourceScopeId,
    SimulationScopeId? DestinationScopeId);

public sealed record WorldManifestState(
    int SchemaVersion,
    string ModelVersion,
    string RulesVersion,
    string ContentVersion,
    ulong WorldSeed,
    SimulationTime Time,
    long CheckpointId,
    ulong NextScopeId,
    long NextEntityId,
    long NextOperationId,
    long OperationReceiptFloor,
    IReadOnlyList<WorldPartitionDescriptor> Partitions,
    IReadOnlyList<WorldEntityLocation> EntityLocations,
    IReadOnlyList<WorldOperationReceipt> OperationReceipts);

public sealed record WorldPartitionState(
    SimulationScopeId ScopeId,
    long Revision,
    SettlementState Settlement);

public sealed record WorldCheckpointState(
    WorldManifestState Manifest,
    IReadOnlyList<WorldPartitionState> Partitions);
