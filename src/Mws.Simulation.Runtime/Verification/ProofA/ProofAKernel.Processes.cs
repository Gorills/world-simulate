using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class ProofAKernel
{
    public long StartLongProcess(
        CommandId commandId,
        EntityId ownerId,
        EntityId subjectId,
        long durationMilliseconds,
        long reservedResource,
        bool interruptible = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(reservedResource);

        if (TryGetRecorded(commandId, out var recorded))
        {
            if (!recorded.Success || recorded.TraceId is null)
            {
                return -1;
            }

            var existing = _pendingProcesses.Values.FirstOrDefault(process => process.StartTraceId == recorded.TraceId);
            return existing?.ProcessId ?? -1;
        }

        if (!_entities.TryGetValue(subjectId.Value, out var entity) || entity.OwnerId != ownerId)
        {
            Record(commandId, false, "OWNER_MISMATCH", null);
            return -1;
        }

        if (entity.Resource < reservedResource)
        {
            Record(commandId, false, "INSUFFICIENT_RESOURCE", null);
            return -1;
        }

        _entities[subjectId.Value] = entity with { Resource = checked(entity.Resource - reservedResource) };
        var traceId = AppendTrace(null, "long-process-started", "Long process started with explicit reservation.");
        var processId = _nextProcessId++;
        var process = new ProofAPendingProcess(
            processId,
            ownerId,
            subjectId,
            Time,
            Time.AddMilliseconds(durationMilliseconds),
            reservedResource,
            interruptible,
            traceId);
        _pendingProcesses.Add(processId, process);
        Record(commandId, true, "PROCESS_STARTED", traceId);
        return processId;
    }

    public ProofACommandExecution InterruptLongProcess(CommandId commandId, EntityId ownerId, long processId)
    {
        if (TryGetRecorded(commandId, out var recorded))
        {
            return recorded;
        }

        if (!_pendingProcesses.TryGetValue(processId, out var process))
        {
            return Record(commandId, false, "PROCESS_NOT_FOUND", null);
        }

        if (process.OwnerId != ownerId)
        {
            return Record(commandId, false, "OWNER_MISMATCH", null);
        }

        if (!process.Interruptible)
        {
            return Record(commandId, false, "PROCESS_NOT_INTERRUPTIBLE", null);
        }

        var entity = _entities[process.SubjectId.Value];
        _entities[process.SubjectId.Value] = entity with { Resource = checked(entity.Resource + process.ReservedResource) };
        _pendingProcesses.Remove(processId);
        var traceId = AppendTrace(process.StartTraceId, "long-process-interrupted", "Reserved resource returned on interruption.");
        return Record(commandId, true, "PROCESS_INTERRUPTED", traceId);
    }

    public void AdvanceTo(SimulationTime target)
    {
        if (target.Milliseconds < Time.Milliseconds)
        {
            throw new InvalidOperationException("Simulation time is monotonic.");
        }

        var due = _pendingProcesses.Values
            .Where(process => process.DueAt.Milliseconds <= target.Milliseconds)
            .OrderBy(process => process.DueAt.Milliseconds)
            .ThenBy(process => process.ProcessId)
            .ToArray();

        foreach (var process in due)
        {
            Time = process.DueAt;
            if (_entities.TryGetValue(process.SubjectId.Value, out var entity))
            {
                _entities[process.SubjectId.Value] = entity with
                {
                    Resource = checked(entity.Resource + checked(process.ReservedResource * 2)),
                };
                AppendTrace(process.StartTraceId, "long-process-completed", "Pending process resumed and completed at canonical time.");
            }

            _pendingProcesses.Remove(process.ProcessId);
        }

        Time = target;
    }

    public ulong ResolveBoundRandom(string domain, EntityId subjectId, long causalAttempt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var key = FormattableString.Invariant($"{domain}|{subjectId.Value}|{causalAttempt}");
        if (_boundRandomOutcomes.TryGetValue(key, out var existing))
        {
            return existing.Value;
        }

        var value = DeterministicSimulationHash.BoundOutcome(_worldSeed, domain, subjectId, causalAttempt);
        _boundRandomOutcomes.Add(key, new ProofABoundRandomOutcome(key, value));
        AppendTrace(null, "random-outcome-bound", "Random outcome bound to represented antecedents.");
        return value;
    }

    private long? AppendTrace(long? parentTraceId, string kind, string detail)
    {
        if (!_traceEnabled)
        {
            return null;
        }

        var traceId = _nextTraceId++;
        _trace.Add(new ProofACausalTraceEntry(traceId, parentTraceId, Time, kind, detail));
        return traceId;
    }
}
