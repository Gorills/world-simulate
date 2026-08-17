using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3RouteAuthorityTests
{
    [Fact]
    public void DestinationRequestDerivesUniqueKnownOpenRoutePathWithoutStartingTravel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9330)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var task = SelectedTask(10, workplace);
        var routes = new[]
        {
            Route(2, SettlementPlaceRef.Settlement, workplace, 700),
            Route(1, home, SettlementPlaceRef.Settlement, 300),
        };
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = WithSelectedTask(state, resident.Id, task),
            RouteConnections = routes,
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [2, 1]),
            ],
        });

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        var request = Assert.IsType<SettlementDestinationRequestProjection>(projected.DestinationRequest);
        var routePath = Assert.IsType<SettlementRoutePathProjection>(projected.RoutePath);

        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Equal(home, location.CurrentPlace);
        Assert.Null(location.Travel);
        Assert.Equal(workplace, request.Destination);
        Assert.Equal(task.TaskId, routePath.TaskId);
        Assert.Equal(home, routePath.Origin);
        Assert.Equal(workplace, routePath.Destination);
        Assert.Equal(new long[] { 1, 2 }, routePath.ConnectionIds);
        Assert.Equal(1_000, routePath.TotalDistanceMeters);
        Assert.Equal(SettlementTravelMode.OnFoot, routePath.TravelMode);

        var captured = simulation.CaptureState();
        Assert.Equal(new long[] { 1, 2 }, captured.RouteConnections!.Select(route => route.ConnectionId));
        Assert.All(
            captured.RouteConnections!,
            route => Assert.Equal(
                new[] { SettlementTravelMode.OnFoot },
                route.SupportedModes!));
        var capturedKnowledge = Assert.Single(captured.ResidentRouteKnowledge!);
        Assert.Equal(new long[] { 1, 2 }, capturedKnowledge.KnownConnectionIds);

        var restored = SettlementSimulation.Restore(
            SettlementStateJson.Deserialize(SettlementStateJson.Serialize(captured)));
        var restoredResident = Assert.Single(restored.Project().Residents, entry => entry.Id == resident.Id);
        var restoredPath = Assert.IsType<SettlementRoutePathProjection>(restoredResident.RoutePath);

        Assert.Equal(routePath.TaskId, restoredPath.TaskId);
        Assert.Equal(routePath.Origin, restoredPath.Origin);
        Assert.Equal(routePath.Destination, restoredPath.Destination);
        Assert.Equal(routePath.ConnectionIds, restoredPath.ConnectionIds);
        Assert.Equal(routePath.TotalDistanceMeters, restoredPath.TotalDistanceMeters);
        Assert.Equal(routePath.TravelMode, restoredPath.TravelMode);
        Assert.Equal(projected.Location, restoredResident.Location);
    }

    [Fact]
    public void MultipleKnownOpenRoutesRemainUnselectedWithoutRouteChoiceModel()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9336)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = WithSelectedTask(state, resident.Id, SelectedTask(13, workplace)),
            RouteConnections =
            [
                Route(1, home, SettlementPlaceRef.Settlement, 300),
                Route(2, SettlementPlaceRef.Settlement, workplace, 700),
                Route(3, home, workplace, 1_200),
            ],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [1, 2, 3]),
            ],
        });

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.NotNull(projected.DestinationRequest);
        Assert.Null(projected.RoutePath);
        Assert.Equal(SettlementActorLocationKind.AtPlace, location.Kind);
        Assert.Equal(home, location.CurrentPlace);
        Assert.Null(location.Travel);
    }

    [Fact]
    public void RestrictedBlockedOrUnknownConnectionsDoNotBecomeRouteAuthority()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9331)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var routes = new[]
        {
            Route(1, home, SettlementPlaceRef.Settlement, 300),
            Route(2, SettlementPlaceRef.Settlement, workplace, 700) with
            {
                PassageStatus = SettlementRoutePassageStatus.Restricted,
            },
            Route(3, home, workplace, 900) with
            {
                PhysicalState = SettlementRoutePhysicalState.Blocked,
            },
            Route(4, SettlementPlaceRef.Settlement, workplace, 650),
        };
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = WithSelectedTask(state, resident.Id, SelectedTask(11, workplace)),
            RouteConnections = routes,
            ResidentRouteKnowledge =
            [
                // Connection 4 is physically/open usable but deliberately unknown to this resident.
                new SettlementResidentRouteKnowledgeState(resident.Id, [1, 2, 3]),
            ],
        });

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);

        Assert.NotNull(projected.DestinationRequest);
        Assert.Null(projected.RoutePath);
    }

    [Fact]
    public void RoutePathRequiresExplicitOnFootModeSupport()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9337)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var taskState = state with
        {
            Residents = WithSelectedTask(state, resident.Id, SelectedTask(14, workplace)),
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [1]),
            ],
        };

        var legacyUndeclared = SettlementSimulation.Restore(taskState with
        {
            RouteConnections =
            [
                Route(1, home, workplace, 500) with { SupportedModes = null },
            ],
        });
        var mountedOnly = SettlementSimulation.Restore(taskState with
        {
            RouteConnections =
            [
                Route(1, home, workplace, 500) with
                {
                    SupportedModes = [SettlementTravelMode.MountedOrAnimalAssisted],
                },
            ],
        });

        Assert.NotNull(legacyUndeclared.Project().Residents[0].DestinationRequest);
        Assert.Null(legacyUndeclared.Project().Residents[0].RoutePath);
        Assert.NotNull(mountedOnly.Project().Residents[0].DestinationRequest);
        Assert.Null(mountedOnly.Project().Residents[0].RoutePath);
    }

    [Fact]
    public void RestoreRejectsInvalidRouteAuthorityState()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9332)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var valid = Route(1, home, workplace, 500);
        var missingHome = new SettlementPlaceRef(SettlementPlaceKind.Home, new EntityId(999_999));

        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections = [valid with { DistanceMeters = 0 }],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections = [valid with { FirstPlace = missingHome }],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections =
            [
                valid,
                Route(1, SettlementPlaceRef.Settlement, workplace, 600),
            ],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections = [valid],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [2]),
            ],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections = [valid with { SupportedModes = [] }],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections =
            [
                valid with
                {
                    SupportedModes =
                    [
                        SettlementTravelMode.OnFoot,
                        SettlementTravelMode.OnFoot,
                    ],
                },
            ],
        }));
        Assert.Throws<InvalidOperationException>(() => SettlementSimulation.Restore(state with
        {
            RouteConnections =
            [
                valid with
                {
                    SupportedModes = [(SettlementTravelMode)999],
                },
            ],
        }));
    }

    [Fact]
    public void ActiveTravelDoesNotCreateASecondRoutePath()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(9333)).CaptureState();
        var resident = state.Residents[0];
        var home = ResidentHome(state, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var travelling = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            home,
            workplace,
            new SettlementTravelProgressState(
                2 * SettlementSimulation.HourMilliseconds,
                ElapsedMilliseconds: 0));
        var residents = state.Residents
            .Select(entry => entry.Id == resident.Id
                ? entry with
                {
                    Location = travelling,
                    SelectedTask = SelectedTask(12, workplace),
                }
                : entry)
            .ToArray();
        var simulation = SettlementSimulation.Restore(state with
        {
            Residents = residents,
            RouteConnections = [Route(1, home, workplace, 500)],
            ResidentRouteKnowledge =
            [
                new SettlementResidentRouteKnowledgeState(resident.Id, [1]),
            ],
        });

        var projected = Assert.Single(simulation.Project().Residents, entry => entry.Id == resident.Id);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);

        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);
        Assert.Null(projected.RoutePath);
    }

    [Fact]
    public void MigrationSafeFailsInsteadOfDroppingResidentRouteKnowledge()
    {
        var world = WorldRuntime.Create(new WorldSeed(9334));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var sourcePartition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == source);
        var resident = sourcePartition.Settlement.Residents[0];
        var home = ResidentHome(sourcePartition.Settlement, resident);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var nextSource = sourcePartition with
        {
            Settlement = sourcePartition.Settlement with
            {
                RouteConnections = [Route(1, home, workplace, 500)],
                ResidentRouteKnowledge =
                [
                    new SettlementResidentRouteKnowledgeState(resident.Id, [1]),
                ],
            },
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
        Assert.Equal("ROUTE_KNOWLEDGE_BLOCKS_MIGRATION", result.Code);
        Assert.True(restored.TryGetEntityLocation(resident.Id, out var actualScope));
        Assert.Equal(source, actualScope);
        var sourceAfter = restored.CaptureSettlementState(source);
        var preserved = Assert.Single(sourceAfter.ResidentRouteKnowledge!);
        Assert.Equal(resident.Id, preserved.ResidentId);
        Assert.Equal(new long[] { 1 }, preserved.KnownConnectionIds);
    }

    [Fact]
    public void EmptyRouteKnowledgeEntryIsRemovedWhenMigrationSucceeds()
    {
        var world = WorldRuntime.Create(new WorldSeed(9335));
        var source = world.AddDefaultSettlement();
        var destination = world.AddDefaultSettlement();
        var checkpoint = world.CaptureCheckpoint();
        var sourcePartition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == source);
        var resident = sourcePartition.Settlement.Residents[0];
        var nextSource = sourcePartition with
        {
            Settlement = sourcePartition.Settlement with
            {
                ResidentRouteKnowledge =
                [
                    new SettlementResidentRouteKnowledgeState(resident.Id, []),
                ],
            },
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

        Assert.True(result.Success);
        Assert.Equal("MIGRATED", result.Code);
        Assert.True(restored.TryGetEntityLocation(resident.Id, out var actualScope));
        Assert.Equal(destination, actualScope);
        Assert.DoesNotContain(
            restored.CaptureSettlementState(source).ResidentRouteKnowledge!,
            entry => entry.ResidentId == resident.Id);
        Assert.DoesNotContain(
            restored.CaptureSettlementState(destination).ResidentRouteKnowledge!,
            entry => entry.ResidentId == resident.Id);
    }

    private static SettlementSelectedTaskState SelectedTask(long taskId, SettlementPlaceRef destination) =>
        new(
            taskId,
            "fixture.explicit-route-task",
            "fixture:test-route-authority",
            new SimulationTime(0),
            destination);

    private static SettlementRouteConnectionState Route(
        long connectionId,
        SettlementPlaceRef first,
        SettlementPlaceRef second,
        long distanceMeters) =>
        new(
            connectionId,
            first,
            second,
            distanceMeters,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            "fixture:test-route-authority",
            IsFixture: true,
            SupportedModes: [SettlementTravelMode.OnFoot]);

    private static ResidentState[] WithSelectedTask(
        SettlementState state,
        EntityId residentId,
        SettlementSelectedTaskState task) =>
        state.Residents
            .Select(entry => entry.Id == residentId ? entry with { SelectedTask = task } : entry)
            .ToArray();

    private static SettlementPlaceRef ResidentHome(SettlementState state, ResidentState resident)
    {
        var household = Assert.Single(state.Households!, entry => entry.Id == resident.HouseholdId);
        return new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
    }
}
