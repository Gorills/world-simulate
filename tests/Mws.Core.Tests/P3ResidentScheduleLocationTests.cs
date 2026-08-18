using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3ResidentScheduleLocationTests
{
    [Fact]
    public void ClockHoursDoNotCreateResidentTravelOrWorkWithoutSelectedTask()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9310));
        var initial = Mira(simulation);
        var initialLocation = RequireLocation(initial);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, initial.HomeId);

        Assert.Equal(SettlementActorLocationKind.AtPlace, initialLocation.Kind);
        Assert.Equal(home, initialLocation.CurrentPlace);
        Assert.Null(initialLocation.Travel);
        Assert.Null(initial.SelectedTask);

        simulation.AdvanceHours(8);
        var morning = Mira(simulation);
        var morningLocation = RequireLocation(morning);

        Assert.Equal(SettlementActorLocationKind.AtPlace, morningLocation.Kind);
        Assert.Equal(home, morningLocation.CurrentPlace);
        Assert.Null(morningLocation.Travel);
        Assert.NotEqual(ResidentActivity.Working, morning.Activity);

        simulation.AdvanceHours(9);
        var evening = Mira(simulation);
        var eveningLocation = RequireLocation(evening);

        Assert.Equal(SettlementActorLocationKind.AtPlace, eveningLocation.Kind);
        Assert.Equal(home, eveningLocation.CurrentPlace);
        Assert.Null(eveningLocation.Travel);
        Assert.NotEqual(ResidentActivity.Working, evening.Activity);
    }

    [Fact]
    public void PersistedPlanlessCompatibilityTravelCanStillFinish()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9311)).CaptureState();
        var resident = state.Residents.Single(entry => entry.Name == "Mira");
        var household = state.Households!.Single(entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var travelling = resident with
        {
            Location = new SettlementActorLocationState(
                SettlementActorLocationKind.Travelling,
                home,
                workplace,
                new SettlementTravelProgressState(
                    SettlementSimulation.HourMilliseconds,
                    0)),
        };
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id ? travelling : entry)
                .ToArray(),
        });

        var serialized = SettlementStateJson.Serialize(simulation.CaptureState());
        simulation = SettlementSimulation.Restore(SettlementStateJson.Deserialize(serialized));

        var before = Mira(simulation);
        var beforeLocation = RequireLocation(before);
        Assert.Equal(SettlementActorLocationKind.Travelling, beforeLocation.Kind);
        Assert.NotNull(beforeLocation.Travel);
        Assert.Null(beforeLocation.Travel.Plan);

        simulation.AdvanceHours(1);

        var arrived = Mira(simulation);
        var arrivedLocation = RequireLocation(arrived);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(workplace, arrivedLocation.CurrentPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.NotEqual(ResidentActivity.Working, arrived.Activity);
    }

    [Fact]
    public void HomeLocationRemainsCompactAcrossClockAdvanceAndRoundTrip()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(9312));
        simulation.AdvanceHours(8);

        var state = simulation.CaptureState();
        Assert.All(state.Residents, resident => Assert.Null(resident.Location));

        var json = SettlementStateJson.Serialize(state);
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));

        Assert.Equal(json, SettlementStateJson.Serialize(restored.CaptureState()));
        Assert.All(restored.Project().Residents, resident =>
        {
            var location = RequireLocation(resident);
            Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
            Assert.Equal(SettlementPlaceKind.Home, location.CurrentPlace.Kind);
            Assert.Equal(resident.HomeId, location.CurrentPlace.EntityId);
            Assert.Null(location.Travel);
        });
    }

    private static ResidentProjection Mira(SettlementSimulation simulation) =>
        simulation.Project().Residents.Single(resident => resident.Name == "Mira");

    private static SettlementActorLocationProjection RequireLocation(ResidentProjection resident) =>
        Assert.IsType<SettlementActorLocationProjection>(resident.Location);
}
