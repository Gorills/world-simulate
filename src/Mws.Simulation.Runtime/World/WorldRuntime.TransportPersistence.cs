using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private void RestoreTransportState(WorldManifestState manifest)
    {
        if (manifest.TransportReceiptFloor <= 0
            || manifest.TransportReceiptFloor > manifest.NextInputSequence
            || (long)manifest.Outbox.Count + manifest.Inbox.Count > MaxPendingTransportMessages
            || manifest.TransportReceipts.Count > MaxRetainedTransportReceipts)
        {
            throw new InvalidOperationException("World transport persistence counters or bounds are invalid.");
        }

        _transportReceiptFloor = manifest.TransportReceiptFloor;
        var retainedInputs = _inputJournal.ToDictionary(entry => entry.Sequence);
        var pendingIds = new HashSet<WorldTransportMessageId>();
        WorldTransportMessageId? previousPendingId = null;

        foreach (var message in manifest.Inbox)
        {
            ValidatePersistedTransportMessage(message, isOutbox: false, retainedInputs);
            EnsureStrictlyIncreasing(previousPendingId, message.MessageId, "pending transport");
            if (!pendingIds.Add(message.MessageId))
            {
                throw new InvalidOperationException("World transport queues contain duplicate message IDs.");
            }

            _inbox.AddLast(message);
            previousPendingId = message.MessageId;
        }

        foreach (var message in manifest.Outbox)
        {
            ValidatePersistedTransportMessage(message, isOutbox: true, retainedInputs);
            EnsureStrictlyIncreasing(previousPendingId, message.MessageId, "pending transport");
            if (!pendingIds.Add(message.MessageId))
            {
                throw new InvalidOperationException("World transport queues contain duplicate message IDs.");
            }

            _outbox.AddLast(message);
            previousPendingId = message.MessageId;
        }

        WorldTransportMessageId? previousReceiptId = null;
        foreach (var receipt in manifest.TransportReceipts)
        {
            ValidatePersistedTransportReceipt(receipt);
            EnsureStrictlyIncreasing(previousReceiptId, receipt.MessageId, "transport receipt");
            if (pendingIds.Contains(receipt.MessageId) || !_transportReceiptIds.Add(receipt.MessageId))
            {
                throw new InvalidOperationException("World transport receipt IDs overlap pending or retained state.");
            }

            _transportReceipts.Enqueue(receipt);
            previousReceiptId = receipt.MessageId;
        }

        if (_transportReceipts.Count == 0 && _transportReceiptFloor != 1)
        {
            throw new InvalidOperationException("Empty world transport receipt history has a non-default floor.");
        }

        if (previousReceiptId is not null && pendingIds.Count > 0)
        {
            var firstPending = _inbox.First?.Value.MessageId ?? _outbox.First!.Value.MessageId;
            if (CompareMessageIds(previousReceiptId, firstPending) >= 0)
            {
                throw new InvalidOperationException("World transport receipts must precede pending messages.");
            }
        }
    }

    private void ValidatePersistedTransportMessage(
        WorldTransportMessage message,
        bool isOutbox,
        Dictionary<long, WorldInputJournalEntry> retainedInputs)
    {
        if (message.MessageId is null
            || message.ResidentMigration is null
            || message.MessageId.SourceInputSequence <= 0
            || message.MessageId.SourceInputSequence == long.MaxValue
            || message.MessageId.SourceInputSequence >= _nextInputSequence
            || message.MessageId.SourceInputSequence < _transportReceiptFloor
            || message.MessageId.Ordinal != 0
            || message.EnqueuedAt.Milliseconds < 0
            || message.EnqueuedAt.Milliseconds > Time.Milliseconds
            || message.EnqueuedAt.Milliseconds % SettlementSimulation.HourMilliseconds != 0
            || message.SourceScopeId.Value == 0
            || message.DestinationScopeId.Value == 0
            || message.SourceScopeId == message.DestinationScopeId
            || message.Kind != WorldTransportMessageKind.ResidentMigration
            || !QueuedResidentMigrationIsValid(message.ResidentMigration)
            || message.ResidentMigration.SourceScopeId != message.SourceScopeId
            || message.ResidentMigration.DestinationScopeId != message.DestinationScopeId
            || message.DeliveryAttempts < 0
            || (isOutbox && message.DeliveryAttempts != 0))
        {
            throw new InvalidOperationException("World transport message state is invalid.");
        }

        if (message.MessageId.SourceInputSequence >= _inputJournalFloor)
        {
            if (!retainedInputs.TryGetValue(message.MessageId.SourceInputSequence, out var input)
                || input.Kind != WorldInputKind.EnqueueResidentMigration
                || input.RecordedAt != message.EnqueuedAt
                || input.EnqueueResidentMigration != message.ResidentMigration)
            {
                throw new InvalidOperationException("World transport message does not match its retained input journal entry.");
            }
        }
    }

    private void ValidatePersistedTransportReceipt(WorldTransportDeliveryReceipt receipt)
    {
        if (receipt.MessageId is null
            || receipt.OperationReceipt is null
            || receipt.MessageId.SourceInputSequence < _transportReceiptFloor
            || receipt.MessageId.SourceInputSequence <= 0
            || receipt.MessageId.SourceInputSequence == long.MaxValue
            || receipt.MessageId.SourceInputSequence >= _nextInputSequence
            || receipt.MessageId.Ordinal != 0
            || receipt.DeliveredAt.Milliseconds < 0
            || receipt.DeliveredAt.Milliseconds > Time.Milliseconds
            || receipt.DeliveredAt.Milliseconds % SettlementSimulation.HourMilliseconds != 0
            || receipt.DeliveryAttempts <= 0
            || !string.Equals(
                receipt.OperationReceipt.Kind,
                WorldOperationKinds.ResidentMigration,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("World transport delivery receipt is invalid.");
        }
    }

    private static void EnsureStrictlyIncreasing(
        WorldTransportMessageId? previous,
        WorldTransportMessageId current,
        string kind)
    {
        if (previous is not null && CompareMessageIds(previous, current) >= 0)
        {
            throw new InvalidOperationException($"World {kind} IDs are not in canonical order.");
        }
    }

    private static int CompareMessageIds(WorldTransportMessageId left, WorldTransportMessageId right)
    {
        var sequence = left.SourceInputSequence.CompareTo(right.SourceInputSequence);
        return sequence != 0 ? sequence : left.Ordinal.CompareTo(right.Ordinal);
    }
}
