using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldRuntimeFoundationTests
{
    [Fact]
    public void DefaultSettlementsUseWorldGlobalEntityIds()
    {
        var world = WorldRuntime.Create(new WorldSeed(100));
        var first = world.AddDefaultSettlement();
        var second = world.AddDefaultSettlement();

        var firstIds = EntityIds(world.CaptureSettlementState(first));
        var secondIds = EntityIds(world.CaptureSettlementState(second));

        Assert.Equal(new SimulationScopeId(1), first);
        Assert.Equal(new SimulationScopeId(2), second);
        Assert.Empty(firstIds.Intersect(secondIds));
        Assert.Equal(firstIds.Count + secondIds.Count, firstIds.Concat(secondIds).Distinct().Count());
    }

    [Fact]
    public void MigrationIsAtomicIdempotentAndPreservesResidentIdentityAndInventory()
    {
        var world = WorldRuntime.Create(new WorldSeed(101));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        world = AddResidentInventoryFixture(world, source);
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;
        var operationId = world.AllocateOperationId();

        var first = world.MigrateResident(operationId, residentId, source, destination);
        var duplicate = world.MigrateResident(operationId, residentId, source, destination);

        Assert.True(first.Success);
        Assert.Equal("MIGRATED", first.Code);
        Assert.Equal(first, duplicate);
        Assert.DoesNotContain(world.CaptureSettlementState(source).Residents, resident => resident.Id == residentId);
        var migrated = Assert.Single(
            world.CaptureSettlementState(destination).Residents,
            resident => resident.Id == residentId);
        Assert.Equal(new EntityId(0), migrated.WorkplaceId);
        Assert.Contains(
            world.CaptureSettlementState(destination).ItemStacks,
            stack => stack.OwnerId == residentId && stack.ItemId == SettlementItems.Ration && stack.Quantity == 1);
        Assert.True(world.TryGetEntityLocation(residentId, out var actualScope));
        Assert.Equal(destination, actualScope);
    }

    [Fact]
    public void MigrationReceiptSurvivesCheckpointRestoreWithoutDoubleApply()
    {
        var world = WorldRuntime.Create(new WorldSeed(102));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var residentId = world.CaptureSettlementState(source).Residents[0].Id;
        var operationId = world.AllocateOperationId();
        var first = world.MigrateResident(operationId, residentId, source, destination);
        var restored = WorldRuntime.Restore(world.CreateCheckpoint());

        var duplicate = restored.MigrateResident(operationId, residentId, source, destination);

        Assert.Equal(first, duplicate);
        Assert.Single(
            restored.CaptureSettlementState(destination).Residents,
            resident => resident.Id == residentId);
    }

    [Fact]
    public void MigrationBatchIsIndependentOfInputIterationOrder()
    {
        var seedWorld = WorldRuntime.Create(new WorldSeed(103));
        var source = seedWorld.AddDefaultSettlement();
        var destination = seedWorld.AddDefaultSettlement();
        var checkpoint = seedWorld.CaptureCheckpoint();
        var residents = seedWorld.CaptureSettlementState(source).Residents.Take(2).Select(entry => entry.Id).ToArray();
        var first = new ResidentMigrationIntent(new WorldOperationId(10), residents[0], source, destination);
        var second = new ResidentMigrationIntent(new WorldOperationId(11), residents[1], source, destination);
        var forward = WorldRuntime.Restore(checkpoint);
        var reverse = WorldRuntime.Restore(checkpoint);

        var forwardResults = forward.ResolveMigrations([first, second]);
        var reverseResults = reverse.ResolveMigrations([second, first]);

        Assert.Equal(forwardResults, reverseResults);
        Assert.Equal(SnapshotSignature(forward), SnapshotSignature(reverse));
    }

    [Fact]
    public void WorldAdvanceFailureDoesNotPartiallyAdvanceEarlierPartition()
    {
        var seedWorld = WorldRuntime.Create(new WorldSeed(104));
        var first = seedWorld.AddDefaultSettlement();
        var second = seedWorld.AddDefaultSettlement();
        seedWorld.AdvanceHours(23);
        var checkpoint = seedWorld.CaptureCheckpoint();
        var poisonedPartitions = checkpoint.Partitions
            .Select(partition => partition.ScopeId == second
                ? partition with { Settlement = partition.Settlement with { NextEventId = long.MaxValue } }
                : partition)
            .ToArray();
        var world = WorldRuntime.Restore(checkpoint with { Partitions = poisonedPartitions });
        var before = SnapshotSignature(world);

        Assert.Throws<InvalidOperationException>(() => world.AdvanceHours(1));

        Assert.Equal(before, SnapshotSignature(world));
        Assert.Equal(23 * SettlementSimulation.HourMilliseconds, world.CaptureSettlementState(first).Time.Milliseconds);
        Assert.Equal(23 * SettlementSimulation.HourMilliseconds, world.CaptureSettlementState(second).Time.Milliseconds);
    }

    [Fact]
    public void JsonWorldStorePersistsPartitionsSeparatelyAndCanLoadOneWithoutTheOther()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mws-world-store-{Guid.NewGuid():N}");
        try
        {
            var world = WorldRuntime.Create(new WorldSeed(105));
            var first = world.AddDefaultSettlement();
            var second = world.AddDefaultSettlement();
            world.AdvanceHours(5);
            var checkpoint = world.CreateCheckpoint();
            var store = new JsonWorldStore(root);
            store.SaveCheckpoint(checkpoint);

            Assert.Equal(
                WorldManifestJson.Serialize(checkpoint.Manifest),
                WorldManifestJson.Serialize(store.LoadManifest()));
            Assert.Equal(
                SettlementStateJson.Serialize(world.CaptureSettlementState(first)),
                SettlementStateJson.Serialize(store.LoadSettlement(first)));

            var secondPath = Path.Combine(
                root,
                $"checkpoint-{checkpoint.Manifest.CheckpointId:D20}",
                "partitions",
                $"scope-{second.Value:D20}.json");
            File.Delete(secondPath);

            _ = store.LoadSettlement(first);
            Assert.Throws<FileNotFoundException>(() => store.LoadCheckpoint());
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
    public void ResidentProjectionCanBePagedWithoutMaterializingWholeSettlementProjection()
    {
        var world = WorldRuntime.Create(new WorldSeed(106));
        var scope = world.AddDefaultSettlement();
        var state = world.CaptureSettlementState(scope);

        var page = world.ProjectResidents(scope, offset: 1, limit: 1);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(1, page.Offset);
        Assert.Single(page.Residents);
        Assert.Equal(state.Residents[1].Id, page.Residents[0].Id);
    }

    private static WorldRuntime AddResidentInventoryFixture(WorldRuntime world, SimulationScopeId source)
    {
        var checkpoint = world.CaptureCheckpoint();
        var partition = checkpoint.Partitions.Single(entry => entry.ScopeId == source);
        var residentId = partition.Settlement.Residents[0].Id;
        var stackId = partition.Settlement.NextStackId;
        var settlement = partition.Settlement with
        {
            NextStackId = checked(stackId + 1),
            ItemStacks = partition.Settlement.ItemStacks
                .Append(new ItemStackState(stackId, SettlementItems.Ration, residentId, 1))
                .ToArray(),
        };
        var partitions = checkpoint.Partitions
            .Select(entry => entry.ScopeId == source ? entry with { Settlement = settlement } : entry)
            .ToArray();
        return WorldRuntime.Restore(checkpoint with { Partitions = partitions });
    }

    private static HashSet<EntityId> EntityIds(SettlementState state) =>
        state.Residents.Select(entry => entry.Id)
            .Concat(state.Workplaces.Select(entry => entry.Id))
            .Append(state.SettlementOwnerId)
            .ToHashSet();

    private static string SnapshotSignature(WorldRuntime world)
    {
        var checkpoint = world.CaptureCheckpoint();
        return string.Join(
            "\n",
            new[] { WorldManifestJson.Serialize(checkpoint.Manifest) }
                .Concat(checkpoint.Partitions
                    .OrderBy(entry => entry.ScopeId.Value)
                    .Select(entry => SettlementStateJson.Serialize(entry.Settlement))));
    }
}
