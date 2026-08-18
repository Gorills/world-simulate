using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3ArrivalActionEligibilityTests
{
    private const string CapabilityProvenance = "fixture:test-p3-arrival-action-capability";
    private const string LoadProvenance = "fixture:test-p3-arrival-action-load";

    [Fact]
    public void ArrivalMakesInteractionPhysicallyEligibleWithoutBypassingSeparateActionCondition()
    {
        var world = WorldRuntime.Create(new WorldSeed(9410));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var partition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == scope);
        var state = partition.Settlement;
        var resident = Assert.Single(state.Residents, entry => entry.Name == "Karo");
        var household = Assert.Single(
            state.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var task = new SettlementSelectedTaskState(
            80,
            "fixture.arrival-action-task",
            "fixture:test-p3-arrival-action",
            new SimulationTime(0),
            SettlementPlaceRef.Settlement);
        var preparedResident = resident with
        {
            SelectedTask = task,
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var route = new SettlementRouteConnectionState(
            91,
            home,
            SettlementPlaceRef.Settlement,
            300,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            "fixture:test-p3-arrival-action-route",
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot],
            OnFootTimingClass: SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed);
        var preparedPartition = partition with
        {
            Settlement = state with
            {
                Residents = state.Residents
                    .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
                    .ToArray(),
                ItemStacks = state.ItemStacks
                    .Where(stack => stack.ItemId != SettlementItems.Ration)
                    .ToArray(),
                RouteConnections = [route],
                ResidentRouteKnowledge =
                [
                    new SettlementResidentRouteKnowledgeState(resident.Id, [route.ConnectionId]),
                ],
            },
        };
        world = WorldRuntime.Restore(checkpoint with
        {
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == scope ? preparedPartition : entry)
                .ToArray(),
        });

        var beforeTravel = world.ExecuteResidentInteraction(
            scope,
            resident.Id,
            ResidentInteractionChoice.ShareRation);
        Assert.False(beforeTravel.Success);
        Assert.Equal(SettlementResultCodes.InteractionNotCoLocated, beforeTravel.Code);

        world.AdvanceHours(1);
        var departed = FindResident(world, scope, resident.Id);
        var departedLocation = Assert.IsType<SettlementActorLocationProjection>(departed.Location);
        var departedTravel = Assert.IsType<SettlementTravelProgressState>(departedLocation.Travel);

        Assert.Equal(SettlementActorLocationKind.Travelling, departedLocation.Kind);
        Assert.Equal(home, departedLocation.CurrentPlace);
        Assert.Equal(SettlementPlaceRef.Settlement, departedLocation.DestinationPlace);
        Assert.Equal(214_286, departedTravel.DurationMilliseconds);
        Assert.Equal(0, departedTravel.ElapsedMilliseconds);

        world.AdvanceTo(world.Time.AddMilliseconds(departedTravel.DurationMilliseconds));

        var arrived = FindResident(world, scope, resident.Id);
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, arrivedLocation.CurrentPlace);
        Assert.Null(arrivedLocation.Travel);

        var afterArrival = world.ExecuteResidentInteraction(
            scope,
            resident.Id,
            ResidentInteractionChoice.ShareRation);

        Assert.False(afterArrival.Success);
        Assert.Equal(SettlementResultCodes.NoRations, afterArrival.Code);
    }

    private static ResidentProjection FindResident(
        WorldRuntime world,
        SimulationScopeId scope,
        EntityId residentId) =>
        Assert.Single(world.ProjectSettlement(scope).Residents, entry => entry.Id == residentId);
}
