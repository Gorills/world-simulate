using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementInvariantTests
{
    [Fact]
    public void TwentySeedsPreserveCoreInvariantsAcrossOneWeek()
    {
        for (ulong seed = 1; seed <= 20; seed++)
        {
            var simulation = SettlementSimulation.CreateDefault(new WorldSeed(seed));

            for (var step = 0; step < 28; step++)
            {
                simulation.AdvanceHours(6);
                var residents = simulation.Project().Residents;
                var residentId = residents[step % residents.Count].Id;
                _ = simulation.Execute(new InteractWithResidentCommand(
                    new CommandId(step + 1),
                    residentId,
                    ResidentInteractionChoice.Encourage));
            }

            AssertStateInvariants(simulation.CaptureState());

            var json = SettlementStateJson.Serialize(simulation.CaptureState());
            var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
            Assert.Equal(json, SettlementStateJson.Serialize(restored.CaptureState()));
        }
    }

    [Fact]
    public void FailedCommandDoesNotMutateProjectedWorldState()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(123));
        var before = simulation.Project();

        var result = simulation.Execute(new GiveItemToResidentCommand(
            new CommandId(10),
            new EntityId(999_999),
            SettlementItems.Herb,
            2));

        var after = simulation.Project();

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.ResidentNotFound, result.Code);
        Assert.Equal(before.Time, after.Time);
        Assert.Equal(before.PantryRations, after.PantryRations);
        Assert.Equal(
            before.Residents
                .Select(resident => (resident.Id, resident.Hunger, resident.Energy, resident.Affinity))
                .ToArray(),
            after.Residents
                .Select(resident => (resident.Id, resident.Hunger, resident.Energy, resident.Affinity))
                .ToArray());
        Assert.Equal(
            before.Stockpile.Select(stack => (stack.StackId, stack.ItemId, stack.Quantity)).ToArray(),
            after.Stockpile.Select(stack => (stack.StackId, stack.ItemId, stack.Quantity)).ToArray());
    }

    private static void AssertStateInvariants(SettlementState state)
    {
        Assert.Equal(state.Residents.Count, state.Residents.Select(resident => resident.Id).Distinct().Count());
        Assert.Equal(state.Workplaces.Count, state.Workplaces.Select(workplace => workplace.Id).Distinct().Count());
        Assert.Equal(state.ItemStacks.Count, state.ItemStacks.Select(stack => stack.StackId).Distinct().Count());
        Assert.Equal(
            state.CommandReceipts.Count,
            state.CommandReceipts.Select(receipt => receipt.CommandId).Distinct().Count());

        Assert.All(state.Residents, resident =>
        {
            Assert.InRange(resident.Hunger, 0, 100);
            Assert.InRange(resident.Energy, 0, 100);
        });
        Assert.All(state.ItemStacks, stack => Assert.True(stack.Quantity >= 0));
    }
}
