using Mws.Domain;

namespace Mws.Simulation.Api;

public sealed record WorldPlayerInventoryItemState(
    string ItemId,
    int Quantity);

public sealed record WorldPlayerActorState(
    EntityId Id,
    SimulationScopeId ScopeId,
    IReadOnlyList<WorldPlayerInventoryItemState> Inventory);

public sealed record WorldPlayerInventoryItemProjection(
    string ItemId,
    int Quantity);

public sealed record WorldPlayerProjection(
    EntityId Id,
    SimulationScopeId ScopeId,
    IReadOnlyList<WorldPlayerInventoryItemProjection> Inventory);

public sealed record WorldAddPlayerActorInput(
    EntityId CreatedPlayerId,
    SimulationScopeId ScopeId);
