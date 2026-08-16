using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementAtomicityTests
{
    [Fact]
    public void EventCapacityFailureDoesNotPartiallyMutateCommandState()
    {
        var baseState = SettlementSimulation.CreateDefault(new WorldSeed(401)).CaptureState();
        var simulation = SettlementSimulation.Restore(baseState with
        {
            NextEventId = long.MaxValue,
        });
        var residentId = simulation.Project().Residents[0].Id;
        var before = simulation.CaptureState();

        Assert.Throws<InvalidOperationException>(() => simulation.FeedResident(residentId));

        var after = simulation.CaptureState();
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.NextEventId, after.NextEventId);
        Assert.Equal(before.Residents.ToArray(), after.Residents.ToArray());
        Assert.Equal(before.ItemStacks.ToArray(), after.ItemStacks.ToArray());
        Assert.Equal(before.Events.ToArray(), after.Events.ToArray());
        Assert.Equal(before.CommandReceipts.ToArray(), after.CommandReceipts.ToArray());
    }

    [Fact]
    public void EventCapacityFailureDoesNotPartiallyTransferInventory()
    {
        var baseState = SettlementSimulation.CreateDefault(new WorldSeed(402)).CaptureState();
        var residentId = baseState.Residents[0].Id;
        var extraStacks = new[]
        {
            new ItemStackState(3, SettlementItems.Herb, baseState.SettlementOwnerId, 1),
        };
        var simulation = SettlementSimulation.Restore(baseState with
        {
            ItemStacks = baseState.ItemStacks.Concat(extraStacks).ToArray(),
            NextStackId = 4,
            NextEventId = long.MaxValue,
        });
        var before = simulation.CaptureState();

        Assert.Throws<InvalidOperationException>(() =>
            simulation.GiveItemToResident(residentId, SettlementItems.Herb, 1));

        var after = simulation.CaptureState();
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.ItemStacks.ToArray(), after.ItemStacks.ToArray());
        Assert.Equal(before.CommandReceipts.ToArray(), after.CommandReceipts.ToArray());
    }
}
