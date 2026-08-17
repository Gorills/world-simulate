using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class DeterminismTests
{
    [Fact]
    public void SameSeedAndCommandStreamProduceSameState()
    {
        var left = SettlementSimulation.CreateDefault(new WorldSeed(42));
        var right = SettlementSimulation.CreateDefault(new WorldSeed(42));

        for (var step = 0; step < 20; step++)
        {
            left.AdvanceHours(6);
            right.AdvanceHours(6);

            var residents = left.Project().Residents;
            var residentId = residents[step % residents.Count].Id;
            var command = new InteractWithResidentCommand(
                new CommandId(step + 1),
                residentId,
                ResidentInteractionChoice.Encourage);

            AssertEquivalent(left.Execute(command), right.Execute(command));
        }

        Assert.Equal(
            SettlementStateJson.Serialize(left.CaptureState()),
            SettlementStateJson.Serialize(right.CaptureState()));
    }

    [Fact]
    public void SaveLoadContinuationMatchesDirectSimulation()
    {
        var direct = SettlementSimulation.CreateDefault(new WorldSeed(7));
        direct.AdvanceHours(240);

        var resumed = SettlementSimulation.CreateDefault(new WorldSeed(7));
        resumed.AdvanceHours(120);
        var json = SettlementStateJson.Serialize(resumed.CaptureState());
        resumed = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        resumed.AdvanceHours(120);

        Assert.Equal(
            SettlementStateJson.Serialize(direct.CaptureState()),
            SettlementStateJson.Serialize(resumed.CaptureState()));
    }

    [Fact]
    public void RepeatedCommandIdIsIdempotentAcrossReload()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(99));
        var residentId = simulation.Project().Residents[0].Id;
        var command = new InteractWithResidentCommand(
            new CommandId(700),
            residentId,
            ResidentInteractionChoice.Encourage);

        var first = simulation.Execute(command);
        var once = SettlementStateJson.Serialize(simulation.CaptureState());
        simulation = SettlementSimulation.Restore(SettlementStateJson.Deserialize(once));
        var second = simulation.Execute(command);
        var twice = SettlementStateJson.Serialize(simulation.CaptureState());

        AssertEquivalent(first, second);
        Assert.Equal(once, twice);
    }

    private static void AssertEquivalent(SettlementCommandResult expected, SettlementCommandResult actual)
    {
        Assert.Equal(expected.Success, actual.Success);
        Assert.Equal(expected.Code, actual.Code);
        Assert.Equal(expected.SubjectId, actual.SubjectId);
        Assert.Equal(expected.Facts.ToArray(), actual.Facts.ToArray());
    }
}
