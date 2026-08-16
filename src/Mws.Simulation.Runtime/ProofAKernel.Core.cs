using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class ProofAKernel
{
    private readonly ulong _worldSeed;
    private readonly bool _traceEnabled;
    private readonly SortedDictionary<long, ProofAEntityState> _entities = new();
    private readonly SortedDictionary<long, ProofATombstone> _tombstones = new();
    private readonly SortedDictionary<long, ProofAPendingProcess> _pendingProcesses = new();
    private readonly SortedDictionary<string, ProofABoundRandomOutcome> _boundRandomOutcomes = new(StringComparer.Ordinal);
    private readonly SortedDictionary<long, ProofACommandExecution> _commandLedger = new();
    private readonly List<ProofACausalTraceEntry> _trace = [];
    private long _nextEntityId = 1;
    private long _nextCommandId = 1;
    private long _nextProcessId = 1;
    private long _nextTraceId = 1;

    public ProofAKernel(WorldSeed seed, bool traceEnabled = true)
    {
        _worldSeed = seed.Value;
        _traceEnabled = traceEnabled;
    }

    private ProofAKernel(ProofAKernelState state, bool traceEnabled)
    {
        _worldSeed = state.WorldSeed;
        _traceEnabled = traceEnabled;
        Time = state.Time;
        _nextEntityId = state.NextEntityId;
        _nextCommandId = state.NextCommandId;
        _nextProcessId = state.NextProcessId;
        _nextTraceId = state.NextTraceId;

        foreach (var entity in state.Entities)
        {
            _entities.Add(entity.Id.Value, entity);
        }

        foreach (var tombstone in state.Tombstones)
        {
            _tombstones.Add(tombstone.Id.Value, tombstone);
        }

        foreach (var process in state.PendingProcesses)
        {
            _pendingProcesses.Add(process.ProcessId, process);
        }

        foreach (var outcome in state.BoundRandomOutcomes)
        {
            _boundRandomOutcomes.Add(outcome.Key, outcome);
        }

        foreach (var execution in state.CommandLedger)
        {
            _commandLedger.Add(execution.CommandId.Value, execution);
        }

        _trace.AddRange(state.Trace.OrderBy(entry => entry.TraceId));
    }

    public SimulationTime Time { get; private set; }

    public int EntityCount => _entities.Count;

    public int PendingProcessCount => _pendingProcesses.Count;

    public int TraceCount => _trace.Count;

    public CommandId AllocateCommandId() => new(_nextCommandId++);

    public EntityId CreateEntity(EntityId? ownerId = null, long initialResource = 0, bool rare = false)
    {
        if (initialResource < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialResource));
        }

        var id = new EntityId(_nextEntityId++);
        var actualOwner = ownerId ?? id;

        if (ownerId is not null && !_entities.ContainsKey(actualOwner.Value))
        {
            throw new InvalidOperationException("Owner entity does not exist.");
        }

        _entities.Add(id.Value, new ProofAEntityState(id, actualOwner, initialResource, rare));
        AppendTrace(null, "entity-created", "Persistent entity created.");
        return id;
    }

    public bool TryGetEntity(EntityId id, out ProofAEntityState? entity) => _entities.TryGetValue(id.Value, out entity);

    public bool TryGetTombstone(EntityId id, out ProofATombstone? tombstone) => _tombstones.TryGetValue(id.Value, out tombstone);
}
