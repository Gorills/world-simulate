using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    public IReadOnlyList<WorldOperationReceipt> ResolveMigrations(IEnumerable<ResidentMigrationIntent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);
        return intents
            .OrderBy(intent => intent.OperationId.Value)
            .ThenBy(intent => intent.ResidentId.Value)
            .ThenBy(intent => intent.SourceScopeId.Value)
            .ThenBy(intent => intent.DestinationScopeId.Value)
            .Select(ResolveMigration)
            .ToArray();
    }

    public WorldOperationReceipt MigrateResident(
        WorldOperationId operationId,
        EntityId residentId,
        SimulationScopeId sourceScopeId,
        SimulationScopeId destinationScopeId) =>
        ResolveMigration(new ResidentMigrationIntent(operationId, residentId, sourceScopeId, destinationScopeId));

    private WorldOperationReceipt ResolveMigration(ResidentMigrationIntent intent)
    {
        if (intent.OperationId.Value <= 0 || intent.OperationId.Value == long.MaxValue)
        {
            return CreateReceipt(intent, false, "INVALID_OPERATION_ID");
        }

        if (intent.OperationId.Value < _operationReceiptFloor)
        {
            return CreateReceipt(intent, false, "STALE_OPERATION");
        }

        if (_operationReceipts.TryGetValue(intent.OperationId.Value, out var recorded))
        {
            return ReceiptMatches(recorded, intent)
                ? recorded
                : CreateReceipt(intent, false, "OPERATION_ID_CONFLICT");
        }

        if (intent.SourceScopeId == intent.DestinationScopeId)
        {
            return RecordOperation(CreateReceipt(intent, false, "SAME_PARTITION"));
        }

        if (!_partitions.TryGetValue(intent.SourceScopeId.Value, out var source)
            || !_partitions.TryGetValue(intent.DestinationScopeId.Value, out var destination))
        {
            return RecordOperation(CreateReceipt(intent, false, "PARTITION_NOT_FOUND"));
        }

        if (!_entityLocations.TryGetValue(intent.ResidentId.Value, out var actualScope))
        {
            return RecordOperation(CreateReceipt(intent, false, "ENTITY_NOT_FOUND"));
        }

        if (actualScope != intent.SourceScopeId)
        {
            return RecordOperation(CreateReceipt(intent, false, "SOURCE_MISMATCH"));
        }

        if (source.Revision == long.MaxValue || destination.Revision == long.MaxValue)
        {
            return RecordOperation(CreateReceipt(intent, false, "REVISION_EXHAUSTED"));
        }

        var sourceState = source.Simulation.CaptureState();
        var destinationState = destination.Simulation.CaptureState();
        var resident = sourceState.Residents.SingleOrDefault(entry => entry.Id == intent.ResidentId);
        if (resident is null)
        {
            return RecordOperation(CreateReceipt(intent, false, "RESIDENT_NOT_FOUND"));
        }

        if (destinationState.Residents.Any(entry => entry.Id == intent.ResidentId))
        {
            return RecordOperation(CreateReceipt(intent, false, "DESTINATION_ALREADY_CONTAINS_ENTITY"));
        }

        var movingStacks = sourceState.ItemStacks
            .Where(stack => stack.OwnerId == intent.ResidentId)
            .OrderBy(stack => stack.StackId)
            .ToArray();
        long destinationNextStackId;
        try
        {
            destinationNextStackId = checked(destinationState.NextStackId + movingStacks.LongLength);
        }
        catch (OverflowException)
        {
            return RecordOperation(CreateReceipt(intent, false, "DESTINATION_STACK_ID_EXHAUSTED"));
        }

        if (movingStacks.Length > 0 && destinationNextStackId <= destinationState.NextStackId)
        {
            return RecordOperation(CreateReceipt(intent, false, "DESTINATION_STACK_ID_EXHAUSTED"));
        }

        var migratedResident = resident with
        {
            Activity = ResidentActivity.Idle,
            WorkplaceId = new EntityId(0),
        };
        var migratedStacks = movingStacks
            .Select((stack, index) => stack with
            {
                StackId = checked(destinationState.NextStackId + index),
            })
            .ToArray();
        var nextSourceState = sourceState with
        {
            Residents = sourceState.Residents.Where(entry => entry.Id != intent.ResidentId).ToArray(),
            ItemStacks = sourceState.ItemStacks.Where(stack => stack.OwnerId != intent.ResidentId).ToArray(),
        };
        var nextDestinationState = destinationState with
        {
            NextStackId = destinationNextStackId,
            Residents = destinationState.Residents.Append(migratedResident).ToArray(),
            ItemStacks = destinationState.ItemStacks.Concat(migratedStacks).ToArray(),
        };

        SettlementSimulation nextSource;
        SettlementSimulation nextDestination;
        try
        {
            nextSource = SettlementSimulation.Restore(nextSourceState);
            nextDestination = SettlementSimulation.Restore(nextDestinationState);
        }
        catch (InvalidOperationException)
        {
            return RecordOperation(CreateReceipt(intent, false, "STATE_VALIDATION_FAILED"));
        }

        source.Simulation = nextSource;
        destination.Simulation = nextDestination;
        source.Revision = checked(source.Revision + 1);
        destination.Revision = checked(destination.Revision + 1);
        _entityLocations[intent.ResidentId.Value] = intent.DestinationScopeId;
        return RecordOperation(CreateReceipt(intent, true, "MIGRATED"));
    }

    private WorldOperationReceipt RecordOperation(WorldOperationReceipt receipt)
    {
        _operationReceipts.Add(receipt.OperationId.Value, receipt);
        _nextOperationId = Math.Max(_nextOperationId, checked(receipt.OperationId.Value + 1));

        while (_operationReceipts.Count > MaxRetainedOperationReceipts)
        {
            var oldest = _operationReceipts.First();
            _operationReceipts.Remove(oldest.Key);
            _operationReceiptFloor = Math.Max(_operationReceiptFloor, checked(oldest.Key + 1));
        }

        return receipt;
    }

    private static WorldOperationReceipt CreateReceipt(
        ResidentMigrationIntent intent,
        bool success,
        string code) =>
        new(
            intent.OperationId,
            WorldOperationKinds.ResidentMigration,
            success,
            code,
            intent.ResidentId,
            intent.SourceScopeId,
            intent.DestinationScopeId);

    private static bool ReceiptMatches(WorldOperationReceipt receipt, ResidentMigrationIntent intent) =>
        string.Equals(receipt.Kind, WorldOperationKinds.ResidentMigration, StringComparison.Ordinal)
        && receipt.SubjectId == intent.ResidentId
        && receipt.SourceScopeId == intent.SourceScopeId
        && receipt.DestinationScopeId == intent.DestinationScopeId;
}
