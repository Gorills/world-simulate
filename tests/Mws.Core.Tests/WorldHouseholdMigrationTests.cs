using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldHouseholdMigrationTests
{
    [Fact]
    public void HouseholdAndHomeEntitiesUseWorldGlobalIdentity()
    {
        var world = WorldRuntime.Create(new WorldSeed(701));
        var first = world.AddDefaultSettlement();
        var second = world.AddDefaultSettlement();
        var firstState = world.CaptureSettlementState(first);
        var secondState = world.CaptureSettlementState(second);

        var firstIds = ResidenceIds(firstState);
        var secondIds = ResidenceIds(secondState);

        Assert.Equal(12, firstIds.Count);
        Assert.Equal(12, secondIds.Count);
        Assert.Empty(firstIds.Intersect(secondIds));
        Assert.All(firstIds, id =>
        {
            Assert.True(world.TryGetEntityLocation(id, out var scope));
            Assert.Equal(first, scope);
        });
        Assert.All(secondIds, id =>
        {
            Assert.True(world.TryGetEntityLocation(id, out var scope));
            Assert.Equal(second, scope);
        });
    }

    [Fact]
    public void MigratingResidentLeavesSourceHouseholdAndBecomesUnassigned()
    {
        var world = WorldRuntime.Create(new WorldSeed(702));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var sourceBefore = world.CaptureSettlementState(source);
        var resident = sourceBefore.Residents.Single(entry => entry.Name == "Mira");
        var household = (sourceBefore.Households ?? [])
            .Single(entry => entry.Id == resident.HouseholdId);

        var result = world.MigrateResident(
            world.AllocateOperationId(),
            resident.Id,
            source,
            destination);

        Assert.True(result.Success);
        var sourceAfter = world.CaptureSettlementState(source);
        var destinationAfter = world.CaptureSettlementState(destination);
        var migrated = destinationAfter.Residents.Single(entry => entry.Id == resident.Id);
        Assert.Equal(default(EntityId), migrated.WorkplaceId);
        Assert.Equal(default(EntityId), migrated.HouseholdId);
        Assert.Contains(sourceAfter.Households ?? [], entry => entry.Id == household.Id);
        Assert.Contains(sourceAfter.Homes ?? [], entry => entry.Id == household.HomeId);
    }

    private static HashSet<EntityId> ResidenceIds(SettlementState state) =>
        (state.Homes ?? [])
            .Select(home => home.Id)
            .Concat((state.Households ?? []).Select(household => household.Id))
            .ToHashSet();
}
