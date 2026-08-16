using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementSimulationTests
{
    [Fact]
    public void ThreeDaySimulationIsDeterministicAcrossSaveLoad()
    {
        var direct = SettlementSimulation.CreateDefault(new WorldSeed(501));
        direct.AdvanceHours(72);

        var resumed = SettlementSimulation.CreateDefault(new WorldSeed(501));
        resumed.AdvanceHours(36);
        var saved = SettlementStateJson.Serialize(resumed.CaptureState());
        resumed = SettlementSimulation.Restore(SettlementStateJson.Deserialize(saved));
        resumed.AdvanceHours(36);

        Assert.Equal(
            SettlementStateJson.Serialize(direct.CaptureState()),
            SettlementStateJson.Serialize(resumed.CaptureState()));
        Assert.Equal(new long[] { 1, 2, 3 }, resumed.Project().Residents.Select(resident => resident.Id.Value).ToArray());
    }

    [Fact]
    public void WorkChangesNeedsAndProducesRations()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(502));
        var before = simulation.Project();

        simulation.AdvanceHours(12);
        var after = simulation.Project();

        Assert.True(after.PantryRations > before.PantryRations);
        Assert.All(after.Residents, resident => Assert.True(resident.Hunger > 0));
        Assert.Contains(after.Residents, resident => resident.Activity == Mws.Simulation.Api.ResidentActivity.Working);
    }

    [Fact]
    public void PlayerCanFeedResidentAndInteractionPersists()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(503));
        simulation.AdvanceHours(16);
        var resident = simulation.Project().Residents[0];
        var pantryBefore = simulation.Project().PantryRations;

        var result = simulation.FeedResident(resident.Id);
        var saved = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(saved));
        var projection = restored.Project();

        Assert.True(result.Success);
        Assert.Equal(pantryBefore - 1, projection.PantryRations);
        Assert.Contains(projection.RecentEvents, entry => entry.Kind == "player-fed" && entry.SubjectId == resident.Id);
    }

    [Fact]
    public void ProjectionUsesCanonicalDayAndHour()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(504));
        simulation.AdvanceHours(27);

        var projection = simulation.Project();

        Assert.Equal(1, projection.Day);
        Assert.Equal(3, projection.Hour);
        Assert.Equal(3, projection.Residents.Count);
        Assert.Contains(projection.RecentEvents, entry => entry.Kind == "day-began");
    }
}
