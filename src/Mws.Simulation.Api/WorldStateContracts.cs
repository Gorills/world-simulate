using System.Text.Json.Serialization;
using Mws.Domain;

namespace Mws.Simulation.Api;

public static class WorldVersions
{
    public const int CurrentSchemaVersion = 1;
    public const string CurrentModelVersion = "world-model-v1";
    public const string CurrentRulesVersion = "world-rules-v1";
    public const string CurrentContentVersion = "world-content-v1";
}

public static class WorldSystemIds
{
    public const string SettlementHourly = "settlement-hourly";
    public const string ResidentMigration = "world-resident-migration";
    public const string Scheduler = "world-scheduler";
}

public static class WorldSystemVersions
{
    public const string SettlementHourly = "1.0.0";
    public const string ResidentMigration = "1.0.0";
    public const string Scheduler = "1.0.0";

    public static IReadOnlyList<WorldSystemVersion> CreateCurrent() =>
        Array.AsReadOnly(new[]
        {
            new WorldSystemVersion(WorldSystemIds.SettlementHourly, SettlementHourly),
            new WorldSystemVersion(WorldSystemIds.ResidentMigration, ResidentMigration),
            new WorldSystemVersion(WorldSystemIds.Scheduler, Scheduler),
        });

    public static bool IsCurrent(IEnumerable<WorldSystemVersion>? versions)
    {
        if (versions is null)
        {
            return false;
        }

        return Canonicalize(versions).SequenceEqual(CreateCurrent());
    }

    public static IReadOnlyList<WorldSystemVersion> RequireCurrent(IEnumerable<WorldSystemVersion>? versions)
    {
        if (versions is null)
        {
            throw new NotSupportedException("World system version bundle is missing.");
        }

        var canonical = Canonicalize(versions);
        if (!canonical.SequenceEqual(CreateCurrent()))
        {
            throw new NotSupportedException("World system version bundle is unsupported.");
        }

        return Array.AsReadOnly(canonical);
    }

    private static WorldSystemVersion[] Canonicalize(IEnumerable<WorldSystemVersion> versions) =>
        versions
            .OrderBy(entry => entry.SystemId, StringComparer.Ordinal)
            .ThenBy(entry => entry.Version, StringComparer.Ordinal)
            .ToArray();
}

public static class WorldPartitionKinds
{
    public const string Settlement = "settlement";
}

public static class WorldOperationKinds
{
    public const string ResidentMigration = "resident-migration";
}

public sealed record WorldSystemVersion(
    string SystemId,
    string Version);

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
    IReadOnlyList<WorldOperationReceipt> OperationReceipts)
{
    private IReadOnlyList<WorldSystemVersion> _systemVersions = WorldSystemVersions.CreateCurrent();

    [JsonRequired]
    public IReadOnlyList<WorldSystemVersion> SystemVersions
    {
        get => _systemVersions;
        init => _systemVersions = WorldSystemVersions.RequireCurrent(value);
    }
}

public sealed record WorldPartitionState(
    SimulationScopeId ScopeId,
    long Revision,
    SettlementState Settlement);

public sealed record WorldCheckpointState(
    WorldManifestState Manifest,
    IReadOnlyList<WorldPartitionState> Partitions);
