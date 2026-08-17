using System.Text.Json.Serialization;
using Mws.Domain;

namespace Mws.Simulation.Api;

public static class WorldVersions
{
    public const int CurrentSchemaVersion = 3;
    public const string CurrentModelVersion = "world-model-v1";
    public const string CurrentRulesVersion = "world-rules-v1";
    public const string CurrentContentVersion = "world-content-v1";
}

public static class WorldSystemIds
{
    public const string SettlementHourly = "settlement-hourly";
    public const string InputJournal = "world-input-journal";
    public const string ResidentMigration = "world-resident-migration";
    public const string Scheduler = "world-scheduler";
    public const string Transport = "world-transport";
}

public static class WorldSystemVersions
{
    public const string SettlementHourly = "1.0.0";
    public const string InputJournal = "1.0.0";
    public const string ResidentMigration = "1.0.0";
    public const string Scheduler = "1.0.0";
    public const string Transport = "1.0.0";

    public static IReadOnlyList<WorldSystemVersion> CreateCurrent() =>
        Array.AsReadOnly(new[]
        {
            new WorldSystemVersion(WorldSystemIds.SettlementHourly, SettlementHourly),
            new WorldSystemVersion(WorldSystemIds.InputJournal, InputJournal),
            new WorldSystemVersion(WorldSystemIds.ResidentMigration, ResidentMigration),
            new WorldSystemVersion(WorldSystemIds.Scheduler, Scheduler),
            new WorldSystemVersion(WorldSystemIds.Transport, Transport),
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

public static class WorldTransportCodes
{
    public const string SourcePartitionUnavailable = "SOURCE_PARTITION_UNAVAILABLE";
    public const string DestinationPartitionUnavailable = "DESTINATION_PARTITION_UNAVAILABLE";
}

public enum WorldInputKind
{
    AddDefaultSettlement,
    AllocateOperationId,
    AdvanceTo,
    SettlementCommand,
    ResidentMigration,
    EnqueueResidentMigration,
    DispatchOutbox,
    DeliverInbox,
}

public enum WorldSettlementCommandKind
{
    FeedResident,
    GiveItemToResident,
    InteractWithResident,
}

public enum WorldTransportMessageKind
{
    ResidentMigration,
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

public sealed record WorldQueuedResidentMigration(
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

public sealed record WorldTransportMessageId(
    long SourceInputSequence,
    int Ordinal);

public sealed record WorldTransportMessage(
    WorldTransportMessageId MessageId,
    SimulationTime EnqueuedAt,
    SimulationScopeId SourceScopeId,
    SimulationScopeId DestinationScopeId,
    WorldTransportMessageKind Kind,
    WorldQueuedResidentMigration ResidentMigration,
    int DeliveryAttempts);

public sealed record WorldTransportDeliveryReceipt(
    WorldTransportMessageId MessageId,
    SimulationTime DeliveredAt,
    int DeliveryAttempts,
    WorldOperationReceipt OperationReceipt);

public sealed record WorldTransportDeliveryBatchResult(
    int CompletedCount,
    int RemainingInboxCount,
    string? BlockedCode,
    IReadOnlyList<WorldTransportDeliveryReceipt> Receipts);

public sealed record WorldAddDefaultSettlementInput(
    SimulationScopeId CreatedScopeId);

public sealed record WorldAllocateOperationIdInput(
    WorldOperationId AllocatedOperationId);

public sealed record WorldAdvanceToInput(
    SimulationTime TargetTime);

public sealed record WorldSettlementCommandInput(
    SimulationScopeId ScopeId,
    WorldSettlementCommandKind CommandKind,
    CommandId CommandId,
    EntityId ResidentId,
    string? ItemId,
    int Quantity,
    ResidentInteractionChoice? InteractionChoice);

public sealed record WorldTransportBatchInput(
    int MaxMessages,
    int ExpectedProcessedCount,
    string? ExpectedBlockedCode);

public sealed record WorldInputJournalEntry(
    long Sequence,
    SimulationTime RecordedAt,
    WorldInputKind Kind,
    WorldAddDefaultSettlementInput? AddDefaultSettlement,
    WorldAllocateOperationIdInput? AllocateOperationId,
    WorldAdvanceToInput? AdvanceTo,
    WorldSettlementCommandInput? SettlementCommand,
    ResidentMigrationIntent? ResidentMigration,
    WorldQueuedResidentMigration? EnqueueResidentMigration,
    WorldTransportBatchInput? DispatchOutbox,
    WorldTransportBatchInput? DeliverInbox);

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
    private IReadOnlyList<WorldInputJournalEntry> _inputJournal =
        Array.AsReadOnly(Array.Empty<WorldInputJournalEntry>());
    private IReadOnlyList<WorldTransportMessage> _outbox =
        Array.AsReadOnly(Array.Empty<WorldTransportMessage>());
    private IReadOnlyList<WorldTransportMessage> _inbox =
        Array.AsReadOnly(Array.Empty<WorldTransportMessage>());
    private IReadOnlyList<WorldTransportDeliveryReceipt> _transportReceipts =
        Array.AsReadOnly(Array.Empty<WorldTransportDeliveryReceipt>());

    [JsonRequired]
    public IReadOnlyList<WorldSystemVersion> SystemVersions
    {
        get => _systemVersions;
        init => _systemVersions = WorldSystemVersions.RequireCurrent(value);
    }

    [JsonRequired]
    public long InputJournalFloor { get; init; } = 1;

    [JsonRequired]
    public long NextInputSequence { get; init; } = 1;

    [JsonRequired]
    public IReadOnlyList<WorldInputJournalEntry> InputJournal
    {
        get => _inputJournal;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _inputJournal = Array.AsReadOnly(value.ToArray());
        }
    }

    [JsonRequired]
    public long TransportReceiptFloor { get; init; } = 1;

    [JsonRequired]
    public IReadOnlyList<WorldTransportMessage> Outbox
    {
        get => _outbox;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _outbox = Array.AsReadOnly(value.ToArray());
        }
    }

    [JsonRequired]
    public IReadOnlyList<WorldTransportMessage> Inbox
    {
        get => _inbox;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _inbox = Array.AsReadOnly(value.ToArray());
        }
    }

    [JsonRequired]
    public IReadOnlyList<WorldTransportDeliveryReceipt> TransportReceipts
    {
        get => _transportReceipts;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _transportReceipts = Array.AsReadOnly(value.ToArray());
        }
    }
}

public sealed record WorldPartitionState(
    SimulationScopeId ScopeId,
    long Revision,
    SettlementState Settlement);

public sealed record WorldCheckpointState(
    WorldManifestState Manifest,
    IReadOnlyList<WorldPartitionState> Partitions);
