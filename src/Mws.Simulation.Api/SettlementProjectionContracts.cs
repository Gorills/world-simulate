using Mws.Domain;

namespace Mws.Simulation.Api;

public sealed record ItemStackProjection(
    long StackId,
    string ItemId,
    int Quantity);

public sealed record WorkplaceProjection(
    EntityId Id,
    string Name,
    ResidentProfession Profession,
    string? InputItemId,
    int InputQuantity,
    string OutputItemId,
    int OutputQuantity);

public sealed record HomeProjection(
    EntityId Id,
    string Name,
    string SpatialKey,
    int Capacity,
    int ResidentCount);

public sealed record HouseholdProjection(
    EntityId Id,
    string Name,
    EntityId HomeId,
    string HomeName,
    IReadOnlyList<EntityId> ResidentIds);

public sealed record ResidentProjection(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity,
    ResidentProfession Profession,
    string WorkplaceName,
    int Affinity,
    IReadOnlyList<ItemStackProjection> Inventory,
    EntityId WorkplaceId = default,
    EntityId HouseholdId = default,
    string HouseholdName = "",
    EntityId HomeId = default,
    string HomeName = "",
    SettlementActorLocationProjection? Location = null);

public sealed record ResidentProjectionPage(
    SimulationScopeId ScopeId,
    SimulationTime Time,
    int Offset,
    int TotalCount,
    IReadOnlyList<ResidentProjection> Residents);

public sealed record SettlementProjection(
    SimulationScopeId ScopeId,
    SimulationTime Time,
    int Day,
    int Hour,
    int PantryRations,
    IReadOnlyList<ItemStackProjection> Stockpile,
    IReadOnlyList<WorkplaceProjection> Workplaces,
    IReadOnlyList<ResidentProjection> Residents,
    IReadOnlyList<SettlementEvent> RecentEvents,
    IReadOnlyList<HomeProjection>? Homes = null,
    IReadOnlyList<HouseholdProjection>? Households = null);
