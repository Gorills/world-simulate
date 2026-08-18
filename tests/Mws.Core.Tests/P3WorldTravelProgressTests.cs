using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3WorldTravelProgressTests
{
    private const string CapabilityProvenance = "fixture:test-world-p3-capability";
    private const string LoadProvenance = "fixture:test-world-p3-load";

    [Fact]
    public void WorldRuntimeCarriesSubHourTravelThroughCheckpointAndReplayWithoutExtraHourlyTicks()
    {
        var world = WorldRuntime.Create(new WorldSeed(9402));
        var scope = world.AddDefaultSettlement();
        world = WorldRuntime.Restore(PrepareDefaultKaroTravel(world.CaptureCheckpoint(), scope));
        var baseline = world.CaptureCheckpoint();
        var residentState = Assert.Single(
            world.CaptureSettlementState(scope).Residents,
            entry => entry.Name == "Karo");
        var residentId = residentState.Id;
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            residentState.WorkplaceId);

        world.AdvanceHours(1);

        var departed = FindResident(world, scope, residentId);
        var departedLocation = Assert.IsType<SettlementActorLocationProjection>(departed.Location);
        var departedTravel = Assert.IsType<SettlementTravelProgressState>(departedLocation.Travel);
        var plan = Assert.IsType<SettlementTravelPlanState>(departedTravel.Plan);

        Assert.Equal(SettlementActorLocationKind.Travelling, departedLocation.Kind);
        Assert.Equal(SettlementSimulation.HourMilliseconds, plan.DepartedAt.Milliseconds);
        Assert.Equal(214_286, departedTravel.DurationMilliseconds);
        Assert.Equal(0, departedTravel.ElapsedMilliseconds);

        var partialTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 100_000));
        world.AdvanceTo(partialTime);

        var partial = FindResident(world, scope, residentId);
        var partialLocation = Assert.IsType<SettlementActorLocationProjection>(partial.Location);
        var partialTravel = Assert.IsType<SettlementTravelProgressState>(partialLocation.Travel);

        Assert.Equal(partialTime, world.Time);
        Assert.Equal(SettlementActorLocationKind.Travelling, partialLocation.Kind);
        Assert.Equal(100_000, partialTravel.ElapsedMilliseconds);
        Assert.Equal(departed.Hunger, partial.Hunger);
        Assert.Equal(departed.Energy, partial.Energy);
        Assert.Equal(departed.Activity, partial.Activity);

        var partialCheckpoint = world.CaptureCheckpoint();
        var restored = WorldRuntime.Restore(partialCheckpoint);
        var restoredPartial = FindResident(restored, scope, residentId);
        var restoredLocation = Assert.IsType<SettlementActorLocationProjection>(restoredPartial.Location);
        var restoredTravel = Assert.IsType<SettlementTravelProgressState>(restoredLocation.Travel);

        Assert.Equal(partialTime, restored.Time);
        Assert.Equal(100_000, restoredTravel.ElapsedMilliseconds);
        Assert.NotNull(restoredTravel.Plan);

        var secondPartialTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + 120_000));
        world.AdvanceTo(secondPartialTime);
        var secondPartial = FindResident(world, scope, residentId);
        var secondPartialLocation = Assert.IsType<SettlementActorLocationProjection>(secondPartial.Location);
        var secondPartialTravel = Assert.IsType<SettlementTravelProgressState>(secondPartialLocation.Travel);

        Assert.Equal(120_000, secondPartialTravel.ElapsedMilliseconds);
        Assert.Equal(departed.Hunger, secondPartial.Hunger);
        Assert.Equal(departed.Energy, secondPartial.Energy);
        Assert.Equal(departed.Activity, secondPartial.Activity);

        var arrivalTime = new SimulationTime(
            checked(plan.DepartedAt.Milliseconds + departedTravel.DurationMilliseconds));
        world.AdvanceTo(arrivalTime);

        var arrived = FindResident(world, scope, residentId);
        var arrivedLocation = Assert.IsType<SettlementActorLocationProjection>(arrived.Location);

        Assert.Equal(arrivalTime, world.Time);
        Assert.Equal(SettlementActorLocationKind.AtPlace, arrivedLocation.Kind);
        Assert.Equal(workplace, arrivedLocation.CurrentPlace);
        Assert.Equal(workplace, arrivedLocation.DestinationPlace);
        Assert.Null(arrivedLocation.Travel);
        Assert.Equal(departed.Hunger, arrived.Hunger);
        Assert.Equal(departed.Energy, arrived.Energy);
        Assert.Equal(departed.Activity, arrived.Activity);

        var finalCheckpoint = world.CaptureCheckpoint();
        var tail = finalCheckpoint.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();

        Assert.Equal(4, tail.Length);
        Assert.All(tail, entry => Assert.Equal(WorldInputKind.AdvanceTo, entry.Kind));
        Assert.Equal(partialTime, tail[2].RecordedAt);
        Assert.Equal(secondPartialTime, tail[3].RecordedAt);

        var replayed = WorldRuntime.ReplayFrom(baseline, tail).CaptureCheckpoint();
        Assert.Equal(CheckpointSignature(finalCheckpoint), CheckpointSignature(replayed));
    }

    private static WorldCheckpointState PrepareDefaultKaroTravel(
        WorldCheckpointState checkpoint,
        SimulationScopeId scope)
    {
        var partition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == scope);
        var state = partition.Settlement;
        var resident = Assert.Single(state.Residents, entry => entry.Name == "Karo");
        var route = Assert.Single(state.RouteConnections!);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            71,
            "fixture.explicit-world-p3-travel-task",
            "fixture:test-world-p3-travel-task",
            state.Time,
            workplace);
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
        var preparedSettlement = state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
                .ToArray(),
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(
                    resident.Id,
                    [route.ConnectionId]),
            ],
        };

        return checkpoint with
        {
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == scope
                    ? entry with { Settlement = preparedSettlement }
                    : entry)
                .ToArray(),
        };
    }

    private static ResidentProjection FindResident(
        WorldRuntime world,
        SimulationScopeId scope,
        EntityId residentId) =>
        Assert.Single(world.ProjectSettlement(scope).Residents, entry => entry.Id == residentId);

    private static string CheckpointSignature(WorldCheckpointState checkpoint) =>
        string.Join(
            "\n",
            new[] { WorldManifestJson.Serialize(checkpoint.Manifest) }
                .Concat(checkpoint.Partitions
                    .OrderBy(entry => entry.ScopeId.Value)
                    .Select(entry => SettlementStateJson.Serialize(entry.Settlement))));
}
