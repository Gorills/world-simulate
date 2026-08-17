using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
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
        Assert.Equal(
            Enumerable.Range(1, 12).Select(index => (long)index).ToArray(),
            resumed.Project().Residents.Select(resident => resident.Id.Value).ToArray());
    }

    [Fact]
    public void DefaultVillageStartsRestingAndTransitionsToWorkAtEight()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(507));
        var midnight = simulation.Project();

        Assert.Equal(0, midnight.Hour);
        Assert.Equal(12, midnight.Residents.Count);
        Assert.All(midnight.Residents, resident => Assert.Equal(ResidentActivity.Resting, resident.Activity));

        simulation.AdvanceHours(8);
        var morning = simulation.Project();

        Assert.Equal(8, morning.Hour);
        Assert.All(morning.Residents, resident => Assert.Equal(ResidentActivity.Working, resident.Activity));
    }

    [Fact]
    public void WorkplacesProduceAndTransformOwnedItemStacks()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(502));

        simulation.AdvanceHours(12);
        var projection = simulation.Project();

        Assert.Equal(3, projection.Workplaces.Count);
        Assert.Contains(projection.Residents, resident => resident.Activity == ResidentActivity.Working);
        Assert.True(StockpileQuantity(projection, SettlementItems.Ration) > 24);
        Assert.True(StockpileQuantity(projection, SettlementItems.Herb) > 0);
        Assert.True(StockpileQuantity(projection, SettlementItems.Grain) >= 16);
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
        Assert.Equal(SettlementResultCodes.FedResident, result.Code);
        Assert.Equal(pantryBefore - 1, projection.PantryRations);
        Assert.Contains(projection.RecentEvents, entry =>
            entry.Kind == SettlementEventKinds.PlayerFed && entry.SubjectId == resident.Id);
    }

    [Fact]
    public void ItemTransferPreservesOwnershipAcrossSaveLoad()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(504));
        simulation.AdvanceHours(12);
        var resident = simulation.Project().Residents[0];
        var stockpileBefore = StockpileQuantity(simulation.Project(), SettlementItems.Herb);

        var result = simulation.GiveItemToResident(resident.Id, SettlementItems.Herb, 2);
        var saved = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(saved));
        var projection = restored.Project();
        var restoredResident = projection.Residents.Single(entry => entry.Id == resident.Id);

        Assert.True(result.Success);
        Assert.Equal(SettlementResultCodes.ItemGiven, result.Code);
        Assert.Equal(stockpileBefore - 2, StockpileQuantity(projection, SettlementItems.Herb));
        Assert.Equal(2, InventoryQuantity(restoredResident, SettlementItems.Herb));
        Assert.Contains(projection.RecentEvents, entry =>
            entry.Kind == SettlementEventKinds.ItemGiven && entry.SubjectId == resident.Id);
    }

    [Fact]
    public void RpgChoiceChangesAffinityAndSurvivesReload()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(505));
        simulation.AdvanceHours(8);
        var resident = simulation.Project().Residents[1];

        var workAnswer = simulation.InteractWithResident(resident.Id, ResidentInteractionChoice.AskAboutWork);
        var encouragement = simulation.InteractWithResident(resident.Id, ResidentInteractionChoice.Encourage);
        var sharedRation = simulation.InteractWithResident(resident.Id, ResidentInteractionChoice.ShareRation);
        var saved = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(saved));
        var restoredResident = restored.Project().Residents.Single(entry => entry.Id == resident.Id);

        Assert.True(workAnswer.Success);
        Assert.Equal(SettlementResultCodes.WorkInfo, workAnswer.Code);
        Assert.Contains(
            workAnswer.Facts,
            fact => fact.Key == SettlementFactKeys.Profession && fact.Value == resident.Profession.ToString());
        Assert.True(encouragement.Success);
        Assert.True(sharedRation.Success);
        Assert.Equal(3, restoredResident.Affinity);
        Assert.Contains(restored.Project().RecentEvents, entry =>
            entry.Kind == SettlementEventKinds.SharedRation && entry.SubjectId == resident.Id);
    }

    [Fact]
    public void ProjectionUsesCanonicalDayHourAndAssignments()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(506));
        simulation.AdvanceHours(27);

        var projection = simulation.Project();

        Assert.Equal(1, projection.Day);
        Assert.Equal(3, projection.Hour);
        Assert.Equal(12, projection.Residents.Count);
        Assert.Equal(3, projection.Workplaces.Count);
        Assert.All(projection.Residents, resident => Assert.False(string.IsNullOrWhiteSpace(resident.WorkplaceName)));
        Assert.Contains(projection.RecentEvents, entry => entry.Kind == SettlementEventKinds.DayBegan);
    }

    private static int StockpileQuantity(SettlementProjection projection, string itemId) =>
        projection.Stockpile
            .Where(stack => string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
            .Sum(stack => stack.Quantity);

    private static int InventoryQuantity(ResidentProjection resident, string itemId) =>
        resident.Inventory
            .Where(stack => string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
            .Sum(stack => stack.Quantity);
}
