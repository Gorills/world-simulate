using System.Text.Json.Serialization;
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
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    SettlementOnFootCarriedLoadClass OnFootCarriedLoad =
        SettlementOnFootCarriedLoadClass.Unknown,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    string? OnFootCarriedLoadProvenanceReference = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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
