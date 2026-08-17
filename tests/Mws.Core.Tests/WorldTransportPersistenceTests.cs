using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldTransportPersistenceTests
{
    [Fact]
    public void InboxSurvivesJsonWorldStoreCheckpointAndCanResumeDelivery()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mws-transport-store-{Guid.NewGuid():N}");
        try
        {
            var world = WorldRuntime.Create(new WorldSeed(8201));
            var source = world.AddDefaultSettlement();
            var destination = world.AddDefaultSettlement();
            var residentId = world.CaptureSettlementState(source).Residents[0].Id;
            var messageId = world.EnqueueResidentMigration(residentId, source, destination);
            Assert.Equal(1, world.DispatchOutbox(1));

            var store = new JsonWorldStore(root);
            store.SaveCheckpoint(world.CreateCheckpoint());
            var restored = WorldRuntime.Restore(store.LoadCheckpoint());

            Assert.Equal(messageId, Assert.Single(restored.CaptureCheckpoint().Manifest.Inbox).MessageId);
            var delivery = restored.DeliverInbox(1);
            Assert.Equal(1, delivery.CompletedCount);
            Assert.True(Assert.Single(delivery.Receipts).OperationReceipt.Success);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void RestoreRejectsMessagePresentInBothOutboxAndInbox()
    {
        var world = WorldRuntime.Create(new WorldSeed(8202));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;
        _ = world.EnqueueResidentMigration(residentId, source, destination);
        var checkpoint = world.CaptureCheckpoint();
        var message = Assert.Single(checkpoint.Manifest.Outbox);
        var corrupted = checkpoint with
        {
            Manifest = checkpoint.Manifest with
            {
                Inbox = [message],
            },
        };

        Assert.Throws<InvalidOperationException>(() => WorldRuntime.Restore(corrupted));
    }

    [Fact]
    public void PendingTransportCapacityIsBoundedWithoutConsumingInputSequenceOnFailure()
    {
        var world = WorldRuntime.Create(new WorldSeed(8203));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;

        for (var index = 0; index < 4_096; index++)
        {
            _ = world.EnqueueResidentMigration(residentId, source, destination);
        }

        var before = world.CaptureCheckpoint();
        Assert.Equal(4_096, before.Manifest.Outbox.Count);
        _ = WorldRuntime.Restore(before);

        Assert.Throws<InvalidOperationException>(() =>
            world.EnqueueResidentMigration(residentId, source, destination));

        var after = world.CaptureCheckpoint();
        Assert.Equal(before.Manifest.NextInputSequence, after.Manifest.NextInputSequence);
        Assert.Equal(4_096, after.Manifest.Outbox.Count);
    }
}
