using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private WorldTransportDeliveryBatchResult DeliverInboxCore(int maxMessages)
    {
        ValidateTransportBatchSize(maxMessages);
        EnsureTransportAttemptCapacity(maxMessages);
        EnsureTransportOperationCapacity(CountImmediatelyDeliverable(maxMessages));
        var receipts = new List<WorldTransportDeliveryReceipt>(Math.Min(maxMessages, _inbox.Count));
        string? blockedCode = null;

        while (receipts.Count < maxMessages && _inbox.First is not null)
        {
            var node = _inbox.First;
            var message = node.Value;
            var attempts = checked(message.DeliveryAttempts + 1);

            if (!_partitions.ContainsKey(message.SourceScopeId.Value))
            {
                node.Value = message with { DeliveryAttempts = attempts };
                blockedCode = WorldTransportCodes.SourcePartitionUnavailable;
                break;
            }

            if (!_partitions.ContainsKey(message.DestinationScopeId.Value))
            {
                node.Value = message with { DeliveryAttempts = attempts };
                blockedCode = WorldTransportCodes.DestinationPartitionUnavailable;
                break;
            }

            if (_transportReceiptIds.Contains(message.MessageId))
            {
                throw new InvalidOperationException("Pending transport message already has a delivery receipt.");
            }

            var operationId = new WorldOperationId(_nextOperationId);
            var operationReceipt = message.Kind switch
            {
                WorldTransportMessageKind.ResidentMigration => ResolveMigrationCore(new ResidentMigrationIntent(
                    operationId,
                    message.ResidentMigration.ResidentId,
                    message.SourceScopeId,
                    message.DestinationScopeId)),
                _ => throw new InvalidOperationException(
                    $"Unknown world transport message kind: {message.Kind}."),
            };

            var receipt = new WorldTransportDeliveryReceipt(
                message.MessageId,
                Time,
                attempts,
                operationReceipt);
            _inbox.RemoveFirst();
            RecordTransportReceipt(receipt);
            receipts.Add(receipt);
        }

        return new WorldTransportDeliveryBatchResult(
            receipts.Count,
            _inbox.Count,
            blockedCode,
            receipts.ToArray());
    }

    private void RecordTransportReceipt(WorldTransportDeliveryReceipt receipt)
    {
        if (!_transportReceiptIds.Add(receipt.MessageId))
        {
            throw new InvalidOperationException("World transport delivery receipt is duplicated.");
        }

        _transportReceipts.Enqueue(receipt);
        while (_transportReceipts.Count > MaxRetainedTransportReceipts)
        {
            var oldest = _transportReceipts.Dequeue();
            _transportReceiptIds.Remove(oldest.MessageId);
            _transportReceiptFloor = Math.Max(
                _transportReceiptFloor,
                checked(oldest.MessageId.SourceInputSequence + 1));
        }
    }

    private void EnsureTransportAttemptCapacity(int maxMessages)
    {
        var inspected = 0;
        var node = _inbox.First;
        while (node is not null && inspected < maxMessages)
        {
            var message = node.Value;
            if (message.DeliveryAttempts == int.MaxValue)
            {
                throw new InvalidOperationException("World transport delivery-attempt space is exhausted.");
            }

            inspected++;
            if (!_partitions.ContainsKey(message.SourceScopeId.Value)
                || !_partitions.ContainsKey(message.DestinationScopeId.Value))
            {
                break;
            }

            node = node.Next;
        }
    }

    private int CountImmediatelyDeliverable(int maxMessages)
    {
        var count = 0;
        var node = _inbox.First;
        while (node is not null && count < maxMessages)
        {
            var message = node.Value;
            if (!_partitions.ContainsKey(message.SourceScopeId.Value)
                || !_partitions.ContainsKey(message.DestinationScopeId.Value))
            {
                break;
            }

            count++;
            node = node.Next;
        }

        return count;
    }

    private void EnsureTransportOperationCapacity(int count)
    {
        if (count < 0
            || (count > 0 && (_nextOperationId <= 0 || _nextOperationId > long.MaxValue - count)))
        {
            throw new InvalidOperationException("World operation ID space cannot satisfy transport delivery batch.");
        }
    }
}
