using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3HumanControllerTravelSymmetryTests
{
    private const string CapabilityProvenance = "fixture:test-p3-human-controller-capability";
    private const string LoadProvenance = "fixture:test-p3-human-controller-load";

    [Fact]
    public void HumanControllerUsesSameRouteCostAndPersistsProgressThroughSaveAndReplay()
    {
        var fixture = Prepare(blockRoute: false);
        var world = fixture.World;
        var baseline = world.CaptureCheckpoint();

        world.SelectPlayerTask(
            91,
            "fixture.human-controller-travel-task",
            "fixture:test-p3-human-controller-travel",
            fixture.Workplace);
        world.AdvanceTo(new SimulationTime(100_000));

        var player = world.ProjectPlayer();
        var playerLocation = Assert.IsType<SettlementActorLocationProjection>(player.Location);
        var playerTravel = Assert.IsType<SettlementTravelProgressState>(playerLocation.Travel);
        var playerPlan = Assert.IsType<SettlementTravelPlanState>(playerTravel.Plan);
        var playerTask = Assert.IsType<SettlementSelectedTaskProjection>(player.SelectedTask);

        Assert.Equal(SettlementActorLocationKind.Travelling, playerLocation.Kind);
        Assert.Equal(fixture.Home, playerLocation.CurrentPlace);
        Assert.Equal(fixture.Workplace, playerLocation.DestinationPlace);
        Assert.Equal(214_286, playerTravel.DurationMilliseconds);
        Assert.Equal(100_000, playerTravel.ElapsedMilliseconds);
        Assert.Equal(91, playerPlan.TaskId);
        Assert.Equal(new SimulationTime(0), playerPlan.DepartedAt);
        Assert.Equal(new[] { fixture.Route.ConnectionId }, playerPlan.ConnectionIds.ToArray());
        Assert.Equal(SettlementTravelMode.OnFoot, playerPlan.TravelMode);
        Assert.Equal(91, playerTask.TaskId);
        Assert.Equal(new[] { fixture.Route.ConnectionId }, player.KnownRouteConnectionIds!.ToArray());

        var residentSimulation = SettlementSimulation.Restore(fixture.PreparedSettlement);
        residentSimulation.AdvanceHours(1);
        var resident = Assert.Single(
            residentSimulation.Project().Residents,
            entry => entry.Id == fixture.ResidentId);
        var residentLocation = Assert.IsType<SettlementActorLocationProjection>(resident.Location);
        var residentTravel = Assert.IsType<SettlementTravelProgressState>(residentLocation.Travel);
        var residentPlan = Assert.IsType<SettlementTravelPlanState>(residentTravel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, residentLocation.Kind);
        Assert.Equal(playerLocation.CurrentPlace, residentLocation.CurrentPlace);
        Assert.Equal(playerLocation.DestinationPlace, residentLocation.DestinationPlace);
        Assert.Equal(playerTravel.DurationMilliseconds, residentTravel.DurationMilliseconds);
        Assert.Equal(playerPlan.ConnectionIds.ToArray(), residentPlan.ConnectionIds.ToArray());
        Assert.Equal(playerPlan.TravelMode, residentPlan.TravelMode);

        var partialCheckpoint = world.CaptureCheckpoint();
        var persisted = WorldRuntime.Restore(JsonRoundTrip(partialCheckpoint));
        AssertPlayerTravelEquivalent(player, persisted.ProjectPlayer());

        var tail = partialCheckpoint.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();
        Assert.Equal(2, tail.Length);
        Assert.Equal(WorldInputKind.SelectPlayerTask, tail[0].Kind);
        Assert.Equal(WorldInputKind.AdvanceTo, tail[1].Kind);
        var replayed = WorldRuntime.ReplayFrom(baseline, tail);
        AssertPlayerTravelEquivalent(player, replayed.ProjectPlayer());

        persisted.AdvanceTo(new SimulationTime(214_286));
        var arrived = persisted.ProjectPlayer();
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(fixture.Workplace, arrivedLocation.CurrentPlace);
        Assert.Equal(fixture.Workplace, arrivedLocation.DestinationPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.Equal(91, Assert.IsType<SettlementSelectedTaskProjection>(arrived.SelectedTask).TaskId);
    }

    [Fact]
    public void HumanControllerCannotBypassBlockedKnownRouteThatAlsoBlocksResidentTravel()
    {
        var fixture = Prepare(blockRoute: true);
        var world = fixture.World;

        world.SelectPlayerTask(
            92,
            "fixture.human-controller-blocked-route-task",
            "fixture:test-p3-human-controller-blocked-route",
            fixture.Workplace);
        world.AdvanceHours(1);

        var player = world.ProjectPlayer();
        var playerLocation = Assert.IsType<SettlementActorLocationProjection>(player.Location);
        Assert.Equal(SettlementActorLocationKind.AtPlace, playerLocation.Kind);
        Assert.Equal(fixture.Home, playerLocation.CurrentPlace);
        Assert.Null(playerLocation.Travel);
        Assert.Equal(92, Assert.IsType<SettlementSelectedTaskProjection>(player.SelectedTask).TaskId);
        Assert.Equal(new[] { fixture.Route.ConnectionId }, player.KnownRouteConnectionIds!.ToArray());

        var resident = Assert.Single(
            world.ProjectSettlement(fixture.Scope).Residents,
            entry => entry.Id == fixture.ResidentId);
        var residentLocation = Assert.IsType<SettlementActorLocationProjection>(resident.Location);
        Assert.Equal(SettlementActorLocationKind.AtPlace, residentLocation.Kind);
        Assert.Equal(fixture.Home, residentLocation.CurrentPlace);
        Assert.Null(residentLocation.Travel);
        Assert.NotNull(resident.SelectedTask);
        Assert.Null(resident.RoutePath);
    }

    private static Fixture Prepare(bool blockRoute)
    {
        var world = WorldRuntime.Create(new WorldSeed(9420));
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
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var route = Assert.Single(state.RouteConnections!);
        if (blockRoute)
        {
            route = route with { PhysicalState = SettlementRoutePhysicalState.Blocked };
        }

        var residentTask = new SettlementSelectedTaskState(
            90,
            "fixture.ai-controller-travel-task",
            "fixture:test-p3-ai-controller-travel",
            new SimulationTime(0),
            workplace);
        var preparedResident = resident with
        {
            SelectedTask = residentTask,
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var preparedSettlement = state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
                .ToArray(),
            RouteConnections = [route],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [route.ConnectionId]),
            ],
        };

        var player = checkpoint.Manifest.Player
            ?? throw new InvalidOperationException("Fixture world did not create a player actor.");
        var preparedPlayer = player with
        {
            Location = SettlementActorLocationState.At(home),
            LocationEncodingVersion = WorldPlayerLocationVersions.CurrentEncodingVersion,
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
            SelectedTask = null,
            KnownRouteConnectionIds = [route.ConnectionId],
        };
        var preparedCheckpoint = checkpoint with
        {
            Manifest = checkpoint.Manifest with { Player = preparedPlayer },
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == scope
                    ? entry with { Settlement = preparedSettlement }
                    : entry)
                .ToArray(),
        };

        return new Fixture(
            WorldRuntime.Restore(preparedCheckpoint),
            scope,
            resident.Id,
            home,
            workplace,
            route,
            preparedSettlement);
    }

    private static WorldCheckpointState JsonRoundTrip(WorldCheckpointState checkpoint)
    {
        var manifest = WorldManifestJson.Deserialize(WorldManifestJson.Serialize(checkpoint.Manifest));
        var partitions = checkpoint.Partitions
            .Select(partition => partition with
            {
                Settlement = SettlementStateJson.Deserialize(
                    SettlementStateJson.Serialize(partition.Settlement)),
            })
            .ToArray();
        return new WorldCheckpointState(manifest, partitions);
    }

    private static void AssertPlayerTravelEquivalent(
        WorldPlayerProjection expected,
        WorldPlayerProjection actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ScopeId, actual.ScopeId);
        Assert.Equal(expected.KnownRouteConnectionIds!.ToArray(), actual.KnownRouteConnectionIds!.ToArray());

        var expectedTask = Assert.IsType<SettlementSelectedTaskProjection>(expected.SelectedTask);
        var actualTask = Assert.IsType<SettlementSelectedTaskProjection>(actual.SelectedTask);
        Assert.Equal(expectedTask, actualTask);

        var expectedLocation = Assert.IsType<SettlementActorLocationProjection>(expected.Location);
        var actualLocation = Assert.IsType<SettlementActorLocationProjection>(actual.Location);
        Assert.Equal(expectedLocation.Kind, actualLocation.Kind);
        Assert.Equal(expectedLocation.CurrentPlace, actualLocation.CurrentPlace);
        Assert.Equal(expectedLocation.DestinationPlace, actualLocation.DestinationPlace);
        var expectedTravel = Assert.IsType<SettlementTravelProgressState>(expectedLocation.Travel);
        var actualTravel = Assert.IsType<SettlementTravelProgressState>(actualLocation.Travel);
        Assert.Equal(expectedTravel.DurationMilliseconds, actualTravel.DurationMilliseconds);
        Assert.Equal(expectedTravel.ElapsedMilliseconds, actualTravel.ElapsedMilliseconds);
        var expectedPlan = Assert.IsType<SettlementTravelPlanState>(expectedTravel.Plan);
        var actualPlan = Assert.IsType<SettlementTravelPlanState>(actualTravel.Plan);
        Assert.Equal(expectedPlan.TaskId, actualPlan.TaskId);
        Assert.Equal(expectedPlan.DepartedAt, actualPlan.DepartedAt);
        Assert.Equal(expectedPlan.ConnectionIds.ToArray(), actualPlan.ConnectionIds.ToArray());
        Assert.Equal(expectedPlan.TravelMode, actualPlan.TravelMode);
    }

    private sealed record Fixture(
        WorldRuntime World,
        SimulationScopeId Scope,
        EntityId ResidentId,
        SettlementPlaceRef Home,
        SettlementPlaceRef Workplace,
        SettlementRouteConnectionState Route,
        SettlementState PreparedSettlement);
}
