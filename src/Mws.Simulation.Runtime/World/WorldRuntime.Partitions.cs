using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private WorldPartitionRuntime GetPartition(SimulationScopeId scopeId)
    {
        if (!_partitions.TryGetValue(scopeId.Value, out var partition))
        {
            throw new KeyNotFoundException($"Settlement scope {scopeId.Value} is not present in this world.");
        }

        return partition;
    }

    private WorldPartitionRuntime GetLoadedPartition(SimulationScopeId scopeId)
    {
        var partition = GetPartition(scopeId);
        if (!partition.IsLoaded)
        {
            throw new InvalidOperationException($"Settlement scope {scopeId.Value} is currently unloaded.");
        }

        return partition;
    }

    private bool IsPartitionLoaded(SimulationScopeId scopeId) =>
        _partitions.TryGetValue(scopeId.Value, out var partition) && partition.IsLoaded;

    private void AddPartition(SettlementSimulation simulation, long revision)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        var state = simulation.CaptureState();
        ValidatePartitionForAdd(state, revision);
        _partitions.Add(state.ScopeId.Value, new WorldPartitionRuntime(simulation, revision));
        RegisterPartitionEntities(state);
    }

    private void AddDormantPartition(SettlementState state, long revision)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidatePartitionForAdd(state, revision);
        _partitions.Add(state.ScopeId.Value, new WorldPartitionRuntime(state, revision));
        RegisterPartitionEntities(state);
    }

    private void ValidatePartitionForAdd(SettlementState state, long revision)
    {
        if (revision < 0
            || state.Time != Time
            || state.WorldSeed != _worldSeed
            || _partitions.ContainsKey(state.ScopeId.Value))
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
    }

    private void RegisterPartitionEntities(SettlementState state)
    {
        foreach (var entityId in EnumerateEntityIds(state).OrderBy(id => id.Value))
        {
            _entityLocations.Add(entityId.Value, state.ScopeId);
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

    private sealed class WorldPartitionRuntime
    {
        private SettlementSimulation? _simulation;

        public WorldPartitionRuntime(SettlementSimulation simulation, long revision)
        {
            ArgumentNullException.ThrowIfNull(simulation);
            ScopeId = simulation.ScopeId;
            _simulation = simulation;
            Revision = revision;
        }

        public WorldPartitionRuntime(SettlementState dormantState, long revision)
        {
            ArgumentNullException.ThrowIfNull(dormantState);
            ScopeId = dormantState.ScopeId;
            DormantState = dormantState;
            Revision = revision;
        }

        public SimulationScopeId ScopeId { get; }

        public bool IsLoaded => _simulation is not null;

        public SettlementSimulation Simulation
        {
            get => _simulation ?? throw new InvalidOperationException("World partition is unloaded.");
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _simulation = value;
                DormantState = null;
                DeferredAdvanceCount = 0;
            }
        }

        public SettlementState? DormantState { get; private set; }

        public long Revision { get; set; }

        public long DeferredAdvanceCount { get; set; }

        public void Unload(SettlementState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            _simulation = null;
            DormantState = state;
            DeferredAdvanceCount = 0;
        }

        public void Load(SettlementSimulation simulation, long revision)
        {
            ArgumentNullException.ThrowIfNull(simulation);
            _simulation = simulation;
            DormantState = null;
            Revision = revision;
            DeferredAdvanceCount = 0;
        }
    }
}
