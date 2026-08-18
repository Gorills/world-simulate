using Mws.Domain;

namespace Mws.Simulation.Api;

public static class WorldPlayerLocationVersions
{
    public const int LegacyEncodingVersion = 0;
    public const int CurrentEncodingVersion = 1;
}

public sealed record WorldPlayerInventoryItemState(
    string ItemId,
    int Quantity);

public sealed record WorldPlayerActorState(
    EntityId Id,
    SimulationScopeId ScopeId,
    IReadOnlyList<WorldPlayerInventoryItemState> Inventory,
    SettlementActorLocationState? Location = null,
    int LocationEncodingVersion = WorldPlayerLocationVersions.LegacyEncodingVersion,
    SettlementOnFootActorCapabilityClass OnFootCapability =
        SettlementOnFootActorCapabilityClass.Unknown,
    string? OnFootCapabilityProvenanceReference = null,
    bool IsOnFootCapabilityFixture = false,
    SettlementOnFootCarriedLoadClass OnFootCarriedLoad =
        SettlementOnFootCarriedLoadClass.Unknown,
    string? OnFootCarriedLoadProvenanceReference = null,
    bool IsOnFootCarriedLoadFixture = false);

public sealed record WorldPlayerInventoryItemProjection(
    string ItemId,
    int Quantity);

public sealed record WorldPlayerProjection(
    EntityId Id,
    SimulationScopeId ScopeId,
    IReadOnlyList<WorldPlayerInventoryItemProjection> Inventory,
    SettlementActorLocationProjection? Location = null);

public sealed record WorldAddPlayerActorInput(
    EntityId CreatedPlayerId,
    SimulationScopeId ScopeId);
