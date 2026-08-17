using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldTransportTests
{
    private static readonly int[] BlockedDeliveryAttempts = [1, 0];
    private static readonly int[] RetriedDeliveryAttempts = [2, 1];

    [Fact]
    public void ResidentMigrationMovesDurablyThroughOutboxAndInbox()
    {
        var world = WorldRuntime.Create(new WorldSeed(8101));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;

        var messageId = world.EnqueueResidentMigration(residentId, source, destination);
        var queued = world.CreateCheckpoint();

        var queuedMessage = Assert.Single(queued.Manifest.Outbox);
        Assert.Equal(messageId, queuedMessage.MessageId);
        Assert.Empty(queued.Manifest.Inbox);
        Assert.Empty(queued.Manifest.TransportReceipts);
        Assert.Equal(1L, queued.Manifest.NextOperationId);

        var dispatchedWorld = WorldRuntime.Restore(queued);
        Assert.Equal(1, dispatchedWorld.DispatchOutbox(1));
        var dispatched = dispatchedWorld.CreateCheckpoint();

        Assert.Empty(dispatched.Manifest.Outbox);
        Assert.Equal(messageId, Assert.Single(dispatched.Manifest.Inbox).MessageId);
        Assert.Equal(1L, dispatched.Manifest.NextOperationId);

        var deliveredWorld = WorldRuntime.Restore(dispatched);
        var delivery = deliveredWorld.DeliverInbox(1);

        Assert.Equal(1, delivery.CompletedCount);
        Assert.Equal(0, delivery.RemainingInboxCount);
        Assert.Null(delivery.BlockedCode);
        var deliveryReceipt = Assert.Single(delivery.Receipts);
        Assert.Equal(messageId, deliveryReceipt.MessageId);
        Assert.Equal(1, deliveryReceipt.DeliveryAttempts);
        Assert.Equal(new WorldOperationId(1), deliveryReceipt.OperationReceipt.OperationId);
        Assert.True(deliveryReceipt.OperationReceipt.Success);
        Assert.Equal("MIGRATED", deliveryReceipt.OperationReceipt.Code);

        var final = deliveredWorld.CaptureCheckpoint();
        Assert.Equal(2L, final.Manifest.NextOperationId);
        Assert.Empty(final.Manifest.Outbox);
        Assert.Empty(final.Manifest.Inbox);
        Assert.Equal(deliveryReceipt, Assert.Single(final.Manifest.TransportReceipts));
        Assert.DoesNotContain(
            deliveredWorld.CaptureSettlementState(source).Residents,
            resident => resident.Id == residentId);
        Assert.Contains(
            deliveredWorld.CaptureSettlementState(destination).Residents,
            resident => resident.Id == residentId);

        var serialized = WorldManifestJson.Serialize(final.Manifest);
        var roundTrip = WorldManifestJson.Deserialize(serialized);
        Assert.Equal(serialized, WorldManifestJson.Serialize(roundTrip));
    }

    [Fact]
    public void UnavailableDestinationBlocksLaterMessagesAndRetriesAfterReload()
    {
        var world = WorldRuntime.Create(new WorldSeed(8102));
        var source = world.AddDefaultSettlement();
        var availableDestination = world.AddDefaultSettlement();
        var missingDestination = new SimulationScopeId(3);
        var residents = world.CaptureSettlementState(source).Residents
            .Take(3)
            .Select(resident => resident.Id)
            .ToArray();

        var firstMessage = world.EnqueueResidentMigration(
            residents[0],
            source,
            missingDestination);
        var secondMessage = world.EnqueueResidentMigration(
            residents[1],
            source,
            availableDestination);
        Assert.Equal(2, world.DispatchOutbox(2));

        var blocked = world.DeliverInbox(2);

        Assert.Equal(0, blocked.CompletedCount);
        Assert.Equal(2, blocked.RemainingInboxCount);
        Assert.Equal(WorldTransportCodes.DestinationPartitionUnavailable, blocked.BlockedCode);
        Assert.Contains(
            world.CaptureSettlementState(source).Residents,
            resident => resident.Id == residents[1]);

        var unrelatedOperationId = world.AllocateOperationId();
        var unrelated = world.MigrateResident(
            unrelatedOperationId,
            residents[2],
            source,
            availableDestination);
        Assert.True(unrelated.Success);

        var blockedState = world.CaptureCheckpoint();
        Assert.Equal(
            new[] { firstMessage, secondMessage },
            blockedState.Manifest.Inbox.Select(message => message.MessageId).ToArray());
        Assert.Equal(
            BlockedDeliveryAttempts,
            blockedState.Manifest.Inbox.Select(message => message.DeliveryAttempts).ToArray());
        Assert.Empty(blockedState.Manifest.TransportReceipts);
        Assert.Single(blockedState.Manifest.OperationReceipts);
        Assert.Equal(2L, blockedState.Manifest.NextOperationId);

        var restored = WorldRuntime.Restore(blockedState);
        Assert.Equal(missingDestination, restored.AddDefaultSettlement());
        var delivered = restored.DeliverInbox(2);

        Assert.Equal(2, delivered.CompletedCount);
        Assert.Equal(0, delivered.RemainingInboxCount);
        Assert.Null(delivered.BlockedCode);
        Assert.Equal(
            new[] { firstMessage, secondMessage },
            delivered.Receipts.Select(receipt => receipt.MessageId).ToArray());
        Assert.Equal(
            RetriedDeliveryAttempts,
            delivered.Receipts.Select(receipt => receipt.DeliveryAttempts).ToArray());
        Assert.Equal(
            new long[] { 2, 3 },
            delivered.Receipts.Select(receipt => receipt.OperationReceipt.OperationId.Value).ToArray());
        Assert.All(delivered.Receipts, receipt => Assert.True(receipt.OperationReceipt.Success));
        Assert.Contains(
            restored.CaptureSettlementState(missingDestination).Residents,
            resident => resident.Id == residents[0]);
        Assert.Contains(
            restored.CaptureSettlementState(availableDestination).Residents,
            resident => resident.Id == residents[1]);
        Assert.Equal(4L, restored.CaptureCheckpoint().Manifest.NextOperationId);
    }

    [Fact]
    public void TransportInputsReplayExactlyWithQueuesReceiptsAndRevisions()
    {
        var world = WorldRuntime.Create(new WorldSeed(8103));
        var baseline = world.CaptureCheckpoint();
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;

        _ = world.EnqueueResidentMigration(residentId, source, destination);
        Assert.Equal(1, world.DispatchOutbox(1));
        var delivery = world.DeliverInbox(1);
        Assert.Equal(1, delivery.CompletedCount);
        world.AdvanceHours(3);

        var expected = world.CaptureCheckpoint();
        var tail = expected.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();
        var replayed = WorldRuntime.ReplayFrom(baseline, tail).CaptureCheckpoint();

        Assert.Equal(CheckpointSignature(expected), CheckpointSignature(replayed));
        Assert.Equal(
            new[]
            {
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.AddDefaultSettlement,
                WorldInputKind.EnqueueResidentMigration,
                WorldInputKind.DispatchOutbox,
                WorldInputKind.DeliverInbox,
                WorldInputKind.AdvanceTo,
            },
            tail.Select(entry => entry.Kind).ToArray());
    }

    private static string CheckpointSignature(WorldCheckpointState checkpoint) =>
        string.Join(
            "\n",
            new[] { WorldManifestJson.Serialize(checkpoint.Manifest) }
                .Concat(checkpoint.Partitions
                    .OrderBy(entry => entry.ScopeId.Value)
                    .Select(entry => SettlementStateJson.Serialize(entry.Settlement))));
}
