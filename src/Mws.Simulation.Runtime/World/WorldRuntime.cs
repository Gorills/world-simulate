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
    private ulong _nextScopeId;
    private long _nextEntityId;
    private long _nextOperationId;
    private long _operationReceiptFloor;
    private long _checkpointId;

    private WorldRuntime(
        ulong worldSeed,
        SimulationTime time,
        ulong nextScopeId,
        long nextEntityId,
        long nextOperationId,
        long operationReceiptFloor,
        long checkpointId)
    {
        if (time.Milliseconds < 0
            || nextScopeId == 0
            || nextEntityId <= 0
            || nextOperationId <= 0
            || operationReceiptFloor <= 0
            || checkpointId < 0)
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
    }

    public SimulationTime Time { get; private set; }

    public IReadOnlyList<SimulationScopeId> Scopes =>
        _partitions.Keys.Select(value => new SimulationScopeId(value)).ToArray();

    public static WorldRuntime Create(WorldSeed seed) =>
        new(seed.Value, new SimulationTime(0), 1, 1, 1, 1, 0);

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
            manifest.CheckpointId);

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

            world.AddPartition(SettlementSimulation.Restore(partitionState.Settlement), partitionState.Revision);
        }

        var expectedLocations = manifest.EntityLocations
            .OrderBy(entry => entry.EntityId.Value)
            .ToArray();
        var actualLocations = world._entityLocations
            .Select(entry => new WorldEntityLocation(new EntityId(entry.Key), entry.Value))
            .ToArray();
        if (!expectedLocations.SequenceEqual(actualLocations))
        {
            throw new InvalidOperationException("World entity directory does not match loaded partition contents.");
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

        world.ValidateCounters();
        return world;
    }

    public SimulationScopeId AddDefaultSettlement()
    {
        if (_nextScopeId == ulong.MaxValue)
        {
            throw new InvalidOperationException("World scope ID space is exhausted.");
        }

        if (_nextEntityId > long.MaxValue - SettlementPrototypeContent.EntityIdSpan)
        {
            throw new InvalidOperationException("World entity ID space cannot reserve another settlement block.");
        }

        var scopeId = new SimulationScopeId(_nextScopeId);
        var blockStart = _nextEntityId;
        var entityIdOffset = checked(blockStart - 1);
        var settlement = SettlementSimulation.CreateDefault(new WorldSeed(_worldSeed), scopeId, entityIdOffset);

        AddPartition(settlement, revision: 0);
        _nextScopeId = checked(_nextScopeId + 1);
        _nextEntityId = checked(blockStart + SettlementPrototypeContent.EntityIdSpan);
        return scopeId;
    }

    public SettlementState CaptureSettlementState(SimulationScopeId scopeId) =>
        GetPartition(scopeId).Simulation.CaptureState();

    public SettlementProjection ProjectSettlement(SimulationScopeId scopeId) =>
        GetPartition(scopeId).Simulation.Project();

    public ResidentProjectionPage ProjectResidents(SimulationScopeId scopeId, int offset, int limit) =>
        GetPartition(scopeId).Simulation.ProjectResidents(offset, limit);

    public bool TryGetEntityLocation(EntityId entityId, out SimulationScopeId scopeId) =>
        _entityLocations.TryGetValue(entityId.Value, out scopeId);

    public WorldOperationId AllocateOperationId()
    {
        if (_nextOperationId <= 0 || _nextOperationId == long.MaxValue)
        {
            throw new InvalidOperationException("World operation ID space is exhausted or invalid.");
        }

        var id = new WorldOperationId(_nextOperationId);
        _nextOperationId = checked(_nextOperationId + 1);
        return id;
    }

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
        var partitionStates = _partitions.Values
            .Select(partition => new WorldPartitionState(
                partition.Simulation.ScopeId,
                partition.Revision,
                partition.Simulation.CaptureState()))
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
            _partitions.Values
                .Select(partition => new WorldPartitionDescriptor(
                    partition.Simulation.ScopeId,
                    WorldPartitionKinds.Settlement,
                    partition.Revision))
                .ToArray(),
            _entityLocations
                .Select(entry => new WorldEntityLocation(new EntityId(entry.Key), entry.Value))
                .ToArray(),
            _operationReceipts.Values.ToArray());
        return new WorldCheckpointState(manifest, partitionStates);
    }

    private WorldPartitionRuntime GetPartition(SimulationScopeId scopeId)
    {
        if (!_partitions.TryGetValue(scopeId.Value, out var partition))
        {
            throw new KeyNotFoundException($"Settlement scope {scopeId.Value} is not loaded in this world.");
        }

        return partition;
    }

    private void AddPartition(SettlementSimulation simulation, long revision)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        var state = simulation.CaptureState();
        if (revision < 0
            || simulation.Time != Time
            || state.WorldSeed != _worldSeed
            || _partitions.ContainsKey(simulation.ScopeId.Value))
        {
            throw new InvalidOperationException("Settlement partition metadata is incompatible with the world runtime.");
        }

        var entityIds = EnumerateEntityIds(state).OrderBy(id => id.Value).ToArray();
        if (entityIds.Any(id => id.Value <= 0)
            || entityIds.Select(id => id.Value).Distinct().Count() != entityIds.Length
            || entityIds.Any(id => _entityLocations.ContainsKey(id.Value)))
        {
            throw new InvalidOperationException("Settlement partition contains entity IDs that are invalid or collide globally.");
        }

        _partitions.Add(simulation.ScopeId.Value, new WorldPartitionRuntime(simulation, revision));
        foreach (var entityId in entityIds)
        {
            _entityLocations.Add(entityId.Value, simulation.ScopeId);
        }
    }

    private void ValidateCounters()
    {
        var maxScope = _partitions.Keys.DefaultIfEmpty(0UL).Max();
        var maxEntity = _entityLocations.Keys.DefaultIfEmpty(0L).Max();
        var maxOperation = _operationReceipts.Keys.DefaultIfEmpty(0L).Max();
        if (_nextScopeId <= maxScope
            || _nextEntityId <= maxEntity
            || _nextOperationId <= maxOperation
            || _operationReceiptFloor > _nextOperationId)
        {
            throw new InvalidOperationException("World checkpoint next-ID markers are not monotonic.");
        }
    }

    private static IEnumerable<EntityId> EnumerateEntityIds(SettlementState state)
    {
        yield return state.SettlementOwnerId;
        foreach (var resident in state.Residents)
        {
            yield return resident.Id;
        }

        foreach (var workplace in state.Workplaces)
        {
            yield return workplace.Id;
        }
    }

    private static void ValidateManifestVersion(WorldManifestState manifest)
    {
        if (manifest.SchemaVersion != WorldVersions.CurrentSchemaVersion
            || !string.Equals(manifest.ModelVersion, WorldVersions.CurrentModelVersion, StringComparison.Ordinal)
            || !string.Equals(manifest.RulesVersion, WorldVersions.CurrentRulesVersion, StringComparison.Ordinal)
            || !string.Equals(manifest.ContentVersion, WorldVersions.CurrentContentVersion, StringComparison.Ordinal))
        {
            throw new NotSupportedException("World checkpoint version bundle is unsupported.");
        }
    }

    private sealed class WorldPartitionRuntime(SettlementSimulation simulation, long revision)
    {
        public SettlementSimulation Simulation { get; set; } = simulation;

        public long Revision { get; set; } = revision;
    }
}
