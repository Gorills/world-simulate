using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private const int MaxPendingTransportMessages = 4_096;
    private const int MaxRetainedTransportReceipts = 4_096;

    private readonly LinkedList<WorldTransportMessage> _outbox = new();
    private readonly LinkedList<WorldTransportMessage> _inbox = new();
    private readonly Queue<WorldTransportDeliveryReceipt> _transportReceipts = new();
    private readonly HashSet<WorldTransportMessageId> _transportReceiptIds = new();
    private long _transportReceiptFloor = 1;

    public WorldTransportMessageId EnqueueResidentMigration(
        EntityId residentId,
        SimulationScopeId sourceScopeId,
        SimulationScopeId destinationScopeId)
    {
        var migration = new WorldQueuedResidentMigration(
            residentId,
            sourceScopeId,
            destinationScopeId);
        if (!QueuedResidentMigrationIsValid(migration))
        {
            throw new ArgumentException("Queued resident migration is invalid.", nameof(residentId));
        }

        EnsureInputJournalCapacity(1);
        EnsurePendingTransportCapacity(1);
        var recordedAt = Time;
        var messageId = EnqueueResidentMigrationCore(migration, _nextInputSequence);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.EnqueueResidentMigration,
            enqueueResidentMigration: migration));
        return messageId;
    }

    public int DispatchOutbox(int maxMessages)
    {
        ValidateTransportBatchSize(maxMessages);
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        var dispatched = DispatchOutboxCore(maxMessages);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.DispatchOutbox,
            dispatchOutbox: new WorldTransportBatchInput(maxMessages, dispatched, null)));
        return dispatched;
    }

    public WorldTransportDeliveryBatchResult DeliverInbox(int maxMessages)
    {
        ValidateTransportBatchSize(maxMessages);
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        var result = DeliverInboxCore(maxMessages);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.DeliverInbox,
            deliverInbox: new WorldTransportBatchInput(
                maxMessages,
                result.CompletedCount,
                result.BlockedCode)));
        return result;
    }

    private WorldTransportMessageId EnqueueResidentMigrationCore(
        WorldQueuedResidentMigration migration,
        long sourceInputSequence)
    {
        if (!QueuedResidentMigrationIsValid(migration)
            || sourceInputSequence <= 0
            || sourceInputSequence == long.MaxValue
            || sourceInputSequence != _nextInputSequence)
        {
            throw new InvalidOperationException("Queued resident migration input is invalid.");
        }

        EnsurePendingTransportCapacity(1);
        var messageId = new WorldTransportMessageId(sourceInputSequence, 0);
        _outbox.AddLast(new WorldTransportMessage(
            messageId,
            Time,
            migration.SourceScopeId,
            migration.DestinationScopeId,
            WorldTransportMessageKind.ResidentMigration,
            migration,
            DeliveryAttempts: 0));
        return messageId;
    }

    private int DispatchOutboxCore(int maxMessages)
    {
        ValidateTransportBatchSize(maxMessages);
        var dispatched = Math.Min(maxMessages, _outbox.Count);
        for (var index = 0; index < dispatched; index++)
        {
            var message = _outbox.First!.Value;
            _outbox.RemoveFirst();
            _inbox.AddLast(message);
        }

        return dispatched;
    }

    private void EnsurePendingTransportCapacity(int count)
    {
        if (count < 0
            || (long)_outbox.Count + _inbox.Count + count > MaxPendingTransportMessages)
        {
            throw new InvalidOperationException("World pending transport capacity is exhausted.");
        }
    }

    private static bool QueuedResidentMigrationIsValid(WorldQueuedResidentMigration? migration) =>
        migration is not null
        && migration.ResidentId.Value > 0
        && migration.SourceScopeId.Value > 0
        && migration.DestinationScopeId.Value > 0
        && migration.SourceScopeId != migration.DestinationScopeId;

    private static void ValidateTransportBatchSize(int maxMessages)
    {
        if (maxMessages <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxMessages),
                maxMessages,
                "Transport batch size must be positive.");
        }
    }
}
