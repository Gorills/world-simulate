using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class ProofAKernel
{
    public ProofACommandExecution DestroyEntity(CommandId commandId, EntityId ownerId, EntityId id, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (TryGetRecorded(commandId, out var recorded))
        {
            return recorded;
        }

        if (!_entities.TryGetValue(id.Value, out var entity))
        {
            return Record(commandId, false, "ENTITY_NOT_FOUND", null);
        }

        if (entity.OwnerId != ownerId)
        {
            return Record(commandId, false, "OWNER_MISMATCH", null);
        }

        if (_pendingProcesses.Values.Any(process => process.SubjectId == id))
        {
            return Record(commandId, false, "PENDING_PROCESS_BLOCKS_DESTROY", null);
        }

        _entities.Remove(id.Value);
        _tombstones.Add(id.Value, new ProofATombstone(id, Time, reason));
        var traceId = AppendTrace(null, "entity-destroyed", reason);
        return Record(commandId, true, "DESTROYED", traceId);
    }

    public ProofACommandExecution AdjustResource(CommandId commandId, EntityId ownerId, EntityId id, long delta)
    {
        if (TryGetRecorded(commandId, out var recorded))
        {
            return recorded;
        }

        if (!_entities.TryGetValue(id.Value, out var entity))
        {
            return Record(commandId, false, "ENTITY_NOT_FOUND", null);
        }

        if (entity.OwnerId != ownerId)
        {
            return Record(commandId, false, "OWNER_MISMATCH", null);
        }

        long next;
        try
        {
            next = checked(entity.Resource + delta);
        }
        catch (OverflowException)
        {
            return Record(commandId, false, "RESOURCE_OVERFLOW", null);
        }

        if (next < 0)
        {
            return Record(commandId, false, "INSUFFICIENT_RESOURCE", null);
        }

        _entities[id.Value] = entity with { Resource = next };
        var traceId = AppendTrace(null, "resource-adjusted", "Owner-mediated resource mutation committed.");
        return Record(commandId, true, "APPLIED", traceId);
    }

    public ProofACommandExecution AtomicTransfer(
        CommandId commandId,
        EntityId ownerId,
        EntityId fromId,
        EntityId toId,
        long amount,
        bool simulateFailure = false)
    {
        if (TryGetRecorded(commandId, out var recorded))
        {
            return recorded;
        }

        if (!TryValidateTransfer(ownerId, fromId, toId, amount, out var from, out var to, out var failureCode))
        {
            return Record(commandId, false, failureCode, null);
        }

        var fromBefore = from.Resource;
        var toBefore = to.Resource;
        _entities[fromId.Value] = from with { Resource = checked(fromBefore - amount) };
        _entities[toId.Value] = to with { Resource = checked(toBefore + amount) };

        if (simulateFailure)
        {
            _entities[fromId.Value] = from with { Resource = fromBefore };
            _entities[toId.Value] = to with { Resource = toBefore };
            var rollbackTraceId = AppendTrace(null, "atomic-transfer-rolled-back", "Compensation restored the pre-operation state.");
            return Record(commandId, false, "ROLLED_BACK", rollbackTraceId);
        }

        var traceId = AppendTrace(null, "atomic-transfer", "Atomic multi-entity transition committed.");
        return Record(commandId, true, "TRANSFERRED", traceId);
    }

    public IReadOnlyList<ProofATransferResolution> ResolveSameTimeTransfers(IEnumerable<ProofATransferIntent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);

        var ordered = intents
            .OrderBy(intent => intent.DueAt.Milliseconds)
            .ThenByDescending(intent => intent.Priority)
            .ThenBy(intent => intent.CommandId.Value)
            .ToArray();
        var resolutions = new List<ProofATransferResolution>(ordered.Length);

        foreach (var group in ordered.GroupBy(intent => intent.DueAt.Milliseconds))
        {
            AdvanceTo(new SimulationTime(group.Key));
            var reservations = new Dictionary<long, long>();
            var accepted = new List<ProofATransferIntent>();

            foreach (var intent in group)
            {
                if (TryGetRecorded(intent.CommandId, out var recorded))
                {
                    resolutions.Add(new ProofATransferResolution(intent.CommandId, recorded.Success, recorded.Code));
                    continue;
                }

                if (!TryValidateTransfer(intent.OwnerId, intent.FromId, intent.ToId, intent.Amount, out var from, out _, out var failureCode))
                {
                    Record(intent.CommandId, false, failureCode, null);
                    resolutions.Add(new ProofATransferResolution(intent.CommandId, false, failureCode));
                    continue;
                }

                reservations.TryGetValue(intent.FromId.Value, out var alreadyReserved);
                if (from.Resource - alreadyReserved < intent.Amount)
                {
                    const string conflictCode = "RESERVATION_CONFLICT";
                    Record(intent.CommandId, false, conflictCode, null);
                    resolutions.Add(new ProofATransferResolution(intent.CommandId, false, conflictCode));
                    continue;
                }

                reservations[intent.FromId.Value] = checked(alreadyReserved + intent.Amount);
                accepted.Add(intent);
            }

            foreach (var intent in accepted)
            {
                var from = _entities[intent.FromId.Value];
                var to = _entities[intent.ToId.Value];
                _entities[intent.FromId.Value] = from with { Resource = checked(from.Resource - intent.Amount) };
                _entities[intent.ToId.Value] = to with { Resource = checked(to.Resource + intent.Amount) };
                var traceId = AppendTrace(null, "same-time-transfer", "Deterministic reservation arbitration committed a transfer.");
                Record(intent.CommandId, true, "TRANSFERRED", traceId);
                resolutions.Add(new ProofATransferResolution(intent.CommandId, true, "TRANSFERRED"));
            }
        }

        return resolutions.OrderBy(result => result.CommandId.Value).ToArray();
    }

    private bool TryValidateTransfer(
        EntityId ownerId,
        EntityId fromId,
        EntityId toId,
        long amount,
        out ProofAEntityState from,
        out ProofAEntityState to,
        out string failureCode)
    {
        from = default!;
        to = default!;
        failureCode = string.Empty;

        if (amount <= 0)
        {
            failureCode = "INVALID_AMOUNT";
            return false;
        }

        if (!_entities.TryGetValue(fromId.Value, out from) || !_entities.TryGetValue(toId.Value, out to))
        {
            failureCode = "ENTITY_NOT_FOUND";
            return false;
        }

        if (from.OwnerId != ownerId || to.OwnerId != ownerId)
        {
            failureCode = "OWNER_MISMATCH";
            return false;
        }

        if (from.Resource < amount)
        {
            failureCode = "INSUFFICIENT_RESOURCE";
            return false;
        }

        try
        {
            _ = checked(to.Resource + amount);
        }
        catch (OverflowException)
        {
            failureCode = "RESOURCE_OVERFLOW";
            return false;
        }

        return true;
    }

    private bool TryGetRecorded(CommandId commandId, out ProofACommandExecution execution) =>
        _commandLedger.TryGetValue(commandId.Value, out execution!);

    private ProofACommandExecution Record(CommandId commandId, bool success, string code, long? traceId)
    {
        var execution = new ProofACommandExecution(commandId, success, code, traceId);
        _commandLedger[commandId.Value] = execution;
        _nextCommandId = Math.Max(_nextCommandId, checked(commandId.Value + 1));
        return execution;
    }
}
