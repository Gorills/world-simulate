using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

namespace Mws.Client.Godot.Session;

internal static class P3TravelPlaytestScenario
{
    private const long TaskId = 3_001;
    private const string TaskKind = "fixture.p3.manual-travel";
    private const string TaskReason = "fixture:playable-p3-manual-travel";
    private const string CapabilityProvenance = "fixture:playable-p3-manual-travel-capability";
    private const string LoadProvenance = "fixture:playable-p3-manual-travel-load";

    internal static P3TravelPlaytestBootstrap Create(WorldSeed seed)
    {
        var world = WorldRuntime.Create(seed);
        var scopeId = world.AddDefaultSettlement();
        var playerId = world.AddPlayerActor(scopeId);
        world.AdvanceHours(PlaytestTimeProfile.StartHour);

        var checkpoint = world.CaptureCheckpoint();
        var partition = checkpoint.Partitions.Single(entry => entry.ScopeId == scopeId);
        var settlement = partition.Settlement;
        var resident = settlement.Residents.Single(entry => entry.Name == "Karo");
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, resident.HomeId);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var route = (settlement.RouteConnections
                ?? Array.Empty<SettlementRouteConnectionState>())
            .Single(connection => Connects(connection, home, workplace));
        var selectedTask = new SettlementSelectedTaskState(
            TaskId,
            TaskKind,
            TaskReason,
            world.Time,
            workplace);
        var preparedResident = resident with
        {
            Location = SettlementActorLocationState.At(home),
            SelectedTask = selectedTask,
            OnFootCapability = SettlementOnFootActorCapabilityClass.BaselineCompatible,
            OnFootCapabilityProvenanceReference = CapabilityProvenance,
            IsOnFootCapabilityFixture = true,
            OnFootCarriedLoad = SettlementOnFootCarriedLoadClass.NoMaterialLoad,
            OnFootCarriedLoadProvenanceReference = LoadProvenance,
            IsOnFootCarriedLoadFixture = true,
        };
        var preparedSettlement = settlement with
        {
            Residents = settlement.Residents
                .Select(entry => entry.Id == resident.Id ? preparedResident : entry)
                .ToArray(),
            ResidentRouteKnowledge = (settlement.ResidentRouteKnowledge
                    ?? Array.Empty<SettlementResidentRouteKnowledgeState>())
                .Where(entry => entry.ResidentId != resident.Id)
                .Append(new SettlementResidentRouteKnowledgeState(
                    resident.Id,
                    [route.ConnectionId]))
                .ToArray(),
        };
        var preparedCheckpoint = checkpoint with
        {
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == scopeId
                    ? entry with { Settlement = preparedSettlement }
                    : entry)
                .ToArray(),
        };
        var restored = WorldRuntime.Restore(preparedCheckpoint);
        return new P3TravelPlaytestBootstrap(
            restored,
            scopeId,
            playerId,
            resident.Id);
    }

    private static bool Connects(
        SettlementRouteConnectionState connection,
        SettlementPlaceRef first,
        SettlementPlaceRef second) =>
        (connection.FirstPlace == first && connection.SecondPlace == second)
        || (connection.FirstPlace == second && connection.SecondPlace == first);
}

internal sealed record P3TravelPlaytestBootstrap(
    WorldRuntime World,
    SimulationScopeId ScopeId,
    EntityId PlayerId,
    EntityId ResidentId);
