using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private const int MaxRetainedOperationReceipts = 4_096;

    private readonly ulong _worldSeed;
    private readonly SortedDictionary<ulong, WorldPartitionRuntime> _partitions = [];
    private readonly SortedDictionary<long, SimulationScopeId> _entityLocations = [];
    private readonly SortedDictionary<long, WorldOperationReceipt> _operationReceipts = [];
    private readonly Queue<WorldInputJournalEntry> _inputJournal = new();
    private ulong _nextScopeId;
    private long _nextEntityId;
    private long _nextOperationId;
    private long _operationReceiptFloor;
    private long _checkpointId;
    private long _inputJournalFloor;
    private long _nextInputSequence;

    private WorldRuntime(
        ulong worldSeed,
        SimulationTime time,
        ulong nextScopeId,
        long nextEntityId,
        long nextOperationId,
        long operationReceiptFloor,
        long checkpointId,
        long inputJournalFloor,
        long nextInputSequence)
    {
        if (time.Milliseconds < 0
            || nextScopeId == 0
            || nextEntityId <= 0
            || nextOperationId <= 0
            || operationReceiptFloor <= 0
            || checkpointId < 0
            || inputJournalFloor <= 0
            || nextInputSequence <= 0
            || inputJournalFloor > nextInputSequence)
        {
            throw new InvalidOperationException("World runtime counters and time must be valid positive monotonic values.");
        }

        _worldSeed = worldSeed;
        Time = time;
        _nextScopeId = nextScopeId;
        _nextEntityId = nextEntityId;
        _nextOperationId = nextOperationId;
        _operationReceiptFloor = operationReceiptFloor;
        _checkpointId = checkpointId;
        _inputJournalFloor = inputJournalFloor;
        _nextInputSequence = nextInputSequence;
    }

    public SimulationTime Time { get; private set; }

    public IReadOnlyList<SimulationScopeId> Scopes =>
        _partitions.Keys.Select(value => new SimulationScopeId(value)).ToArray();

    public static WorldRuntime Create(WorldSeed seed) =>
        new(seed.Value, new SimulationTime(0), 1, 1, 1, 1, 0, 1, 1);

    public static WorldRuntime Restore(WorldCheckpointState checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ValidateManifestVersion(checkpoint.Manifest);

        var manifest = checkpoint.Manifest;
        var world = new WorldRuntime(
            manifest.WorldSeed,
            manifest.Time,
            manifest.NextScopeId,
            manifest.NextEntityId,
            manifest.NextOperationId,
            manifest.OperationReceiptFloor,
            manifest.CheckpointId,
            manifest.InputJournalFloor,
            manifest.NextInputSequence);

        var descriptors = manifest.Partitions.ToDictionary(descriptor => descriptor.ScopeId.Value);
        if (descriptors.Count != manifest.Partitions.Count || checkpoint.Partitions.Count != descriptors.Count)
        {
            throw new InvalidOperationException("World checkpoint partition descriptors are incomplete or duplicated.");
        }

        foreach (var partitionState in checkpoint.Partitions.OrderBy(entry => entry.ScopeId.Value))
        {
            if (!descriptors.TryGetValue(partitionState.ScopeId.Value, out var descriptor)
                || !string.Equals(descriptor.Kind, WorldPartitionKinds.Settlement, StringComparison.Ordinal)
                || descriptor.Revision != partitionState.Revision
                || partitionState.Settlement.ScopeId != partitionState.ScopeId
                || partitionState.Settlement.WorldSeed != manifest.WorldSeed
                || partitionState.Settlement.Time != manifest.Time)
            {
                throw new InvalidOperationException("World checkpoint partition metadata does not match its settlement state.");
            }

            if (descriptor.IsLoaded)
            {
                world.AddPartition(SettlementSimulation.Restore(partitionState.Settlement), partitionState.Revision);
            }
            else
            {
                world.AddDormantPartition(partitionState.Settlement, partitionState.Revision);
            }
        }

        var expectedLocations = manifest.EntityLocations
            .OrderBy(entry => entry.EntityId.Value)
            .ToArray();
        var actualLocations = world._entityLocations
            .Select(entry => new WorldEntityLocation(new EntityId(entry.Key), entry.Value))
            .ToArray();
        if (!expectedLocations.SequenceEqual(actualLocations))
        {
            throw new InvalidOperationException("World entity directory does not match partition contents.");
        }

        foreach (var receipt in manifest.OperationReceipts.OrderBy(entry => entry.OperationId.Value))
        {
            if (receipt.OperationId.Value < manifest.OperationReceiptFloor
                || receipt.OperationId.Value <= 0
                || receipt.OperationId.Value == long.MaxValue
                || world._operationReceipts.ContainsKey(receipt.OperationId.Value))
            {
                throw new InvalidOperationException("World operation receipt history is invalid.");
            }

            world._operationReceipts.Add(receipt.OperationId.Value, receipt);
        }

        world.RestoreInputJournal(manifest.InputJournal);
        world.RestoreTransportState(manifest);
        world.ValidateCounters();
        return world;
    }

    public SettlementState CaptureSettlementState(SimulationScopeId scopeId) =>
        CapturePartitionStateAtCurrentTime(GetPartition(scopeId));

    public SettlementProjection ProjectSettlement(SimulationScopeId scopeId) =>
        GetLoadedPartition(scopeId).Simulation.Project();

    public ResidentProjectionPage ProjectResidents(SimulationScopeId scopeId, int offset, int limit) =>
        GetLoadedPartition(scopeId).Simulation.ProjectResidents(offset, limit);

    public bool TryGetEntityLocation(EntityId entityId, out SimulationScopeId scopeId) =>
        _entityLocations.TryGetValue(entityId.Value, out scopeId);

    public WorldCheckpointState CreateCheckpoint()
    {
        if (_checkpointId == long.MaxValue)
        {
            throw new InvalidOperationException("World checkpoint ID space is exhausted.");
        }

        _checkpointId = checked(_checkpointId + 1);
        return CaptureCheckpoint();
    }

    public WorldCheckpointState CaptureCheckpoint()
    {
        var materialized = _partitions.Values
            .Select(partition => (
                Partition: partition,
                State: CapturePartitionStateAtCurrentTime(partition),
                Revision: EffectiveRevision(partition)))
            .ToArray();
        var partitionStates = materialized
            .Select(entry => new WorldPartitionState(
                entry.Partition.ScopeId,
                entry.Revision,
                entry.State))
            .ToArray();
        var manifest = new WorldManifestState(
            WorldVersions.CurrentSchemaVersion,
            WorldVersions.CurrentModelVersion,
            WorldVersions.CurrentRulesVersion,
            WorldVersions.CurrentContentVersion,
            _worldSeed,
            Time,
            _checkpointId,
            _nextScopeId,
            _nextEntityId,
            _nextOperationId,
            _operationReceiptFloor,
            materialized
                .Select(entry => new WorldPartitionDescriptor(
                    entry.Partition.ScopeId,
                    WorldPartitionKinds.Settlement,
                    entry.Revision,
                    entry.Partition.IsLoaded))
                .ToArray(),
            _entityLocations
                .Select(entry => new WorldEntityLocation(new EntityId(entry.Key), entry.Value))
                .ToArray(),
            _operationReceipts.Values.ToArray())
        {
            SystemVersions = WorldSystemVersions.CreateCurrent(),
            InputJournalFloor = _inputJournalFloor,
            NextInputSequence = _nextInputSequence,
            InputJournal = _inputJournal.ToArray(),
            TransportReceiptFloor = _transportReceiptFloor,
            Outbox = _outbox.ToArray(),
            Inbox = _inbox.ToArray(),
            TransportReceipts = _transportReceipts.ToArray(),
        };
        return new WorldCheckpointState(manifest, partitionStates);
    }

    private void ValidateCounters()
    {
        var maxScope = _partitions.Keys.DefaultIfEmpty(0UL).Max();
        var maxEntity = _entityLocations.Keys.DefaultIfEmpty(0L).Max();
        var maxOperation = _operationReceipts.Keys.DefaultIfEmpty(0L).Max();
        if (_nextScopeId <= maxScope
            || _nextEntityId <= maxEntity
            || _nextOperationId <= maxOperation
            || _operationReceiptFloor > _nextOperationId
            || _inputJournalFloor <= 0
            || _nextInputSequence <= 0
            || _inputJournalFloor > _nextInputSequence
            || _transportReceiptFloor <= 0
            || _transportReceiptFloor > _nextInputSequence)
        {
            throw new InvalidOperationException("World checkpoint next-ID markers are not monotonic.");
        }
    }

    private static void ValidateManifestVersion(WorldManifestState manifest)
    {
        if (manifest.SchemaVersion != WorldVersions.CurrentSchemaVersion
            || !string.Equals(manifest.ModelVersion, WorldVersions.CurrentModelVersion, StringComparison.Ordinal)
            || !string.Equals(manifest.RulesVersion, WorldVersions.CurrentRulesVersion, StringComparison.Ordinal)
            || !string.Equals(manifest.ContentVersion, WorldVersions.CurrentContentVersion, StringComparison.Ordinal)
            || !WorldSystemVersions.IsCurrent(manifest.SystemVersions))
        {
            throw new NotSupportedException("World checkpoint version bundle is unsupported.");
        }
    }
}
