using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3SelectedTaskDestinationTests
{
    [Fact]
    public void SelectedTaskPersistsAndProducesDestinationRequestWithoutClockDrivenCommute()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9320)).CaptureState();
        var resident = state.Residents[0];
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var task = new SettlementSelectedTaskState(
            TaskId: 1,
            Kind: "fixture.explicit-work",
            ReasonReference: "fixture:test-selected-task",
            SelectedAt: new SimulationTime(0),
            RequiredPlace: workplace);
        var simulation = SettlementSimulation.Restore(WithSelectedTask(state, resident.Id, task));

        simulation.AdvanceHours(8);

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        var selectedTask = Assert.IsType<SettlementSelectedTaskProjection>(projected.SelectedTask);
        var request = Assert.IsType<SettlementDestinationRequestProjection>(projected.DestinationRequest);
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);

        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Equal(new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId), location.CurrentPlace);
        Assert.Null(location.Travel);
        Assert.Equal(task.TaskId, selectedTask.TaskId);
        Assert.Equal(task.Kind, selectedTask.Kind);
        Assert.Equal(task.ReasonReference, selectedTask.ReasonReference);
        Assert.Equal(task.SelectedAt, selectedTask.SelectedAt);
        Assert.Equal(workplace, selectedTask.RequiredPlace);
        Assert.Equal(task.TaskId, request.TaskId);
        Assert.Equal(workplace, request.Destination);

        var captured = simulation.CaptureState();
        var capturedResident = Assert.Single(captured.Residents, entry => entry.Id == resident.Id);
        Assert.Equal(task, capturedResident.SelectedTask);

        var json = SettlementStateJson.Serialize(captured);
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));
        var restoredResident = Assert.Single(restored.Project().Residents, entry => entry.Id == resident.Id);

        Assert.Equal(projected.SelectedTask, restoredResident.SelectedTask);
        Assert.Equal(projected.DestinationRequest, restoredResident.DestinationRequest);
        Assert.Equal(projected.Location, restoredResident.Location);
    }

    [Fact]
    public void SelectedTaskAtCurrentPlaceDoesNotRequestTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9321)).CaptureState();
        var resident = state.Residents[0];
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var task = new SettlementSelectedTaskState(
            TaskId: 2,
            Kind: "fixture.local-task",
            ReasonReference: "fixture:test-local-task",
            SelectedAt: new SimulationTime(0),
            RequiredPlace: home);
        var simulation = SettlementSimulation.Restore(WithSelectedTask(state, resident.Id, task));

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);

        Assert.NotNull(projected.SelectedTask);
        Assert.Null(projected.DestinationRequest);
    }

    [Fact]
    public void RestoreRejectsInvalidSelectedTaskState()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9322)).CaptureState();
        var resident = state.Residents[0];
        var invalidId = new SettlementSelectedTaskState(
            TaskId: 0,
            Kind: "fixture.invalid",
            ReasonReference: "fixture:test-invalid",
            SelectedAt: new SimulationTime(0),
            RequiredPlace: SettlementPlaceRef.Settlement);
        var missingPlace = invalidId with
        {
            TaskId = 3,
            RequiredPlace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, new EntityId(999_999)),
        };

        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(WithSelectedTask(state, resident.Id, invalidId)));
        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(WithSelectedTask(state, resident.Id, missingPlace)));
    }

    [Fact]
    public void ExistingTravelProgressContinuesWhenSelectedTaskAppears()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9323)).CaptureState();
        var resident = state.Residents[0];
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var travelling = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            home,
            SettlementPlaceRef.Settlement,
            new SettlementTravelProgressState(
                2 * SettlementSimulation.HourMilliseconds,
                ElapsedMilliseconds: 0));
        var task = new SettlementSelectedTaskState(
            TaskId: 4,
            Kind: "fixture.explicit-work",
            ReasonReference: "fixture:test-selected-task",
            SelectedAt: new SimulationTime(0),
            RequiredPlace: workplace);
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id
                ? entry with { Location = travelling, SelectedTask = task }
                : entry)
            .ToArray();
        var simulation = SettlementSimulation.Restore(state with { Residents = residents });

        simulation.AdvanceHours(1);

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);
        Assert.Equal(SettlementPlaceRef.Settlement, location.DestinationPlace);
        Assert.NotNull(location.Travel);
        Assert.Equal(SettlementSimulation.HourMilliseconds, location.Travel.ElapsedMilliseconds);
        Assert.Equal(task.TaskId, projected.SelectedTask?.TaskId);
        Assert.Equal(workplace, projected.DestinationRequest?.Destination);
    }

    [Fact]
    public void MigrationSafeFailsWhileSelectedTaskIsActive()
    {
        var world = WorldRuntime.Create(new WorldSeed(9324));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var sourcePartition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == source);
        var resident = sourcePartition.Settlement.Residents[0];
        var task = new SettlementSelectedTaskState(
            TaskId: 5,
            Kind: "fixture.local-task",
            ReasonReference: "fixture:test-migration-block",
            SelectedAt: new SimulationTime(0),
            RequiredPlace: SettlementPlaceRef.Settlement);
        var nextSource = sourcePartition with
        {
            Settlement = WithSelectedTask(sourcePartition.Settlement, resident.Id, task),
        };
        var partitions = checkpoint.Partitions
            .Select(entry => entry.ScopeId == source ? nextSource : entry)
            .ToArray();
        var restored = WorldRuntime.Restore(checkpoint with { Partitions = partitions });

        var result = restored.MigrateResident(
            restored.AllocateOperationId(),
            resident.Id,
            source,
            destination);

        Assert.False(result.Success);
        Assert.Equal("ACTIVE_TASK_BLOCKS_MIGRATION", result.Code);
        Assert.True(restored.TryGetEntityLocation(resident.Id, out var actualScope));
        Assert.Equal(source, actualScope);
        var preserved = Assert.Single(
            restored.CaptureSettlementState(source).Residents,
            entry => entry.Id == resident.Id);
        Assert.Equal(task, preserved.SelectedTask);
    }

    private static SettlementState WithSelectedTask(
        SettlementState state,
        EntityId residentId,
        SettlementSelectedTaskState task) =>
        state with
        {
            Residents = state.Residents
                .Select(entry => entry.Id == residentId ? entry with { SelectedTask = task } : entry)
                .ToArray(),
        };
}
