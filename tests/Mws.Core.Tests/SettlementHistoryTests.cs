using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementHistoryTests
{
    [Fact]
    public void HistoryRemainsBoundedAndPrunedCommandsCannotExecuteAgain()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(303));
        var residentId = simulation.Project().Residents[0].Id;
        var totalCommands = SettlementSimulation.MaxRetainedCommandReceipts + 32;

        for (var commandValue = 1; commandValue <= totalCommands; commandValue++)
        {
            var result = simulation.Execute(new InteractWithResidentCommand(
                new CommandId(commandValue),
                residentId,
                ResidentInteractionChoice.Encourage));
            Assert.True(result.Success);
        }

        var state = simulation.CaptureState();
        Assert.Equal(SettlementSimulation.MaxRetainedCommandReceipts, state.CommandReceipts.Count);
        Assert.True(state.Events.Count <= SettlementSimulation.MaxRetainedEvents);
        Assert.DoesNotContain(state.CommandReceipts, receipt => receipt.CommandId == new CommandId(1));

        var affinityBefore = simulation.Project().Residents.Single(resident => resident.Id == residentId).Affinity;
        var stale = simulation.Execute(new InteractWithResidentCommand(
            new CommandId(1),
            residentId,
            ResidentInteractionChoice.Encourage));

        Assert.False(stale.Success);
        Assert.Equal(SettlementResultCodes.StaleCommand, stale.Code);
        Assert.Equal(affinityBefore, simulation.Project().Residents.Single(resident => resident.Id == residentId).Affinity);
    }

    [Fact]
    public void RetainedDuplicateCommandRemainsIdempotentAfterHistoryCompaction()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(304));
        var residentId = simulation.Project().Residents[1].Id;
        var totalCommands = SettlementSimulation.MaxRetainedCommandReceipts + 8;
        SettlementCommandResult? expected = null;
        var retainedId = new CommandId(totalCommands);

        for (var commandValue = 1; commandValue <= totalCommands; commandValue++)
        {
            var command = new InteractWithResidentCommand(
                new CommandId(commandValue),
                residentId,
                ResidentInteractionChoice.Encourage);
            var result = simulation.Execute(command);
            if (command.Id == retainedId)
            {
                expected = result;
            }
        }

        Assert.NotNull(expected);
        var affinityBefore = simulation.Project().Residents.Single(resident => resident.Id == residentId).Affinity;
        var duplicate = simulation.Execute(new InteractWithResidentCommand(
            retainedId,
            residentId,
            ResidentInteractionChoice.Encourage));

        Assert.Equal(expected!.Success, duplicate.Success);
        Assert.Equal(expected.Code, duplicate.Code);
        Assert.Equal(expected.SubjectId, duplicate.SubjectId);
        Assert.Equal(expected.Facts.ToArray(), duplicate.Facts.ToArray());
        Assert.Equal(affinityBefore, simulation.Project().Residents.Single(resident => resident.Id == residentId).Affinity);
    }
}
