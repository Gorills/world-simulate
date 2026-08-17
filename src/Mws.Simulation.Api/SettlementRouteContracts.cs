using Mws.Domain;

namespace Mws.Simulation.Api;

public enum SettlementRoutePhysicalState
{
    Passable,
    Blocked,
}

public enum SettlementRoutePassageStatus
{
    Open,
    Restricted,
}

public sealed record SettlementRouteConnectionState(
    long ConnectionId,
    SettlementPlaceRef FirstPlace,
    SettlementPlaceRef SecondPlace,
    long DistanceMeters,
    SettlementRoutePhysicalState PhysicalState,
    SettlementRoutePassageStatus PassageStatus,
    string ProvenanceReference,
    bool IsFixture = false);

public sealed record SettlementResidentRouteKnowledgeState(
    EntityId ResidentId,
    IReadOnlyList<long> KnownConnectionIds);

public sealed record SettlementRoutePathProjection(
    long TaskId,
    SettlementPlaceRef Origin,
    SettlementPlaceRef Destination,
    IReadOnlyList<long> ConnectionIds,
    long TotalDistanceMeters);
